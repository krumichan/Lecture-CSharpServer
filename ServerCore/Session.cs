using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
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
        }

        #region Network 통신 (Send).
        void RegisterSend()
        {
            /*byte[] buffer = _sendQueue.Dequeue();*/
            /*_sendArgs.SetBuffer(buffer, 0, buffer.Length);*/

            // BufferList를 쓸 경우, SetBuffer로 다른 buffer를 설정하면 오류가 난다.
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buffer = _sendQueue.Dequeue();
                _pendingList.Add(buffer);
            }
            _sendArgs.BufferList = _pendingList;

            // Async 처리는 Kernel 단계에서 하기 때문에 부하가 크다.
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
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
            _receiveBuffer.Clean();

            ArraySegment<byte> segment = _receiveBuffer.WriteSegment;
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count); // Offset: 시작 위치, Count: 빈 공간.

            // Async 처리는 Kernel 단계에서 하기 때문에 부하가 크다.
            bool pending = _socket.ReceiveAsync(_receiveArgs);
            if (pending == false)
            {
                OnReceiveCompleted(null, _receiveArgs);
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
