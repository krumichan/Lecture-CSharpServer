using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS ( Domain Name System )
            // Server Address   → Name Address
            // ex) 172.217.26.4 → www.google.com
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // ipAddr: 식당 주소,  7777: 식당 문 위치.

            // 문지기가 들고 있는 Socket.
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 문지기 교육.
                listenSocket.Bind(endPoint);

                // 영업 시작.
                listenSocket.Listen(10); // param: 최대 대기수.

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 손님 입장. (Session Socket 지급)
                    Socket clientSocket = listenSocket.Accept();

                    // 수신.
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);  // param[1]: start index, param[2]: 읽어들일 바이트 수.
                    Console.WriteLine($"[From Client] {recvData}");

                    // 송신.
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                    clientSocket.Send(sendBuffer);

                    // 퇴출.
                    clientSocket.Shutdown(SocketShutdown.Both); // 예고. ( 없어도 됨 )
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
