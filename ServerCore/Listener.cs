using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육.
            _listenSocket.Bind(endPoint);

            // 영업 시작.
            _listenSocket.Listen(10); // param: 최대 대기수.

            SocketAsyncEventArgs args = new SocketAsyncEventArgs(); // 재사용 가능.
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // args는 재사용하기 때문에 비워주어야 함.
            args.AcceptSocket = null;

            // 요청이 즉시 올수도, 안올수도 있음.
            // pending == false: 즉시 도착.
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 요청이 정상 도착.
            if (args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            // Error 발생.
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 다음 Client 대기 요청.
            RegisterAccept(args);
        }
    }
}
