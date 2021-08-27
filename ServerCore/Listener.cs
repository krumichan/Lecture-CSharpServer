using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육.
            _listenSocket.Bind(endPoint);

            // 영업 시작.
            // param: 최대 대기수.
            _listenSocket.Listen(backlog);

            // 독립적인 객체이기 때문에 아래와 같이 여러개를 놓을 수 있다.
            // ( 허용량이 높아지는 것 )
            for (int i = 0; i < register; ++i)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs(); // 재사용 가능.
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
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
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
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
