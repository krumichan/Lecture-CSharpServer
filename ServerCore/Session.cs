using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // sealed: 이후 상속 불가.
        // [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ] ...
        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLength = 0;

            while (true)
            {
                // 최소한 Header는 Parsing 할 수 있는지 확인.
                if (buffer.Count < HeaderSize)
                {
                    break;
                }

                // Packet이 완전체로 도착했는지 확인.
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                {
                    break;
                }

                // Packet 조립 가능.
                // ArraySegment는 Structure로 Heap 영역에 할당되는게 아닌 Stack 영역 복사이다.
                OnReceivePacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLength += dataSize;

                // 읽어들인 size/packetId/data 세트 다음의 새로운 size/packetId/data 머리로 이동.
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLength;
        }

        // [size(2)][packageId(2)][ ... ] → 유효 범위 처리.
        public abstract void OnReceivePacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;  // 0: false, 1: true

        ReceiveBuffer _receiveBuffer = new ReceiveBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnReceive(ArraySegment<byte> buffer);
        public abstract void OnSend(int numberOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            // 임의의 정보를 넣어줄 수 있다.
            /*receiveArgs.UserToken = this;*/
            /*receiveArgs.UserToken = 1;*/

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);

            RegisterReceive();
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_pendingList.Count == 0)
                {
                    // RegisterSend도 lock을 같이 잠금.
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }

            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both); // 예고. ( 없어도 됨 )
            _socket.Close();
            Clear();
        }

        #region Network 통신 (Send).
        void RegisterSend()
        {
            // 최소한의 방어 처리.
            if (_disconnected == 1)
            {
                return;
            }

            /*byte[] buffer = _sendQueue.Dequeue();*/
            /*_sendArgs.SetBuffer(buffer, 0, buffer.Length);*/

            // BufferList를 쓸 경우, SetBuffer로 다른 buffer를 설정하면 오류가 난다.
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buffer = _sendQueue.Dequeue();
                _pendingList.Add(buffer);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                // Async 처리는 Kernel 단계에서 하기 때문에 부하가 크다.
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed.. {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // 예약된 것은 전부 성공적으로 보냈기 때문에 비운다.
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // 지금 온 Send를 처리하는 과정에서
                        // Send Queue에 Msg가 쌓여 있을 경우.
                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }
        #endregion

        #region Network 통신 (Receive).
        void RegisterReceive()
        {
            if (_disconnected == 1)
            {
                return;
            }

            _receiveBuffer.Clean();

            ArraySegment<byte> segment = _receiveBuffer.WriteSegment;
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count); // Offset: 시작 위치, Count: 빈 공간.

            try
            {
                // Async 처리는 Kernel 단계에서 하기 때문에 부하가 크다.
                bool pending = _socket.ReceiveAsync(_receiveArgs);
                if (pending == false)
                {
                    OnReceiveCompleted(null, _receiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterReceive Failed.. {e}");
            }
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 연결을 끊을 때 등 가끔 0이 올 수 있음.
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write Cursor 이동.
                    if (_receiveBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Contents 쪽으로 Data를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLength = OnReceive(_receiveBuffer.ReadSegment);
                    if (processLength < 0 || _receiveBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // Read Cursor 이동.
                    if (_receiveBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnReceiveCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
