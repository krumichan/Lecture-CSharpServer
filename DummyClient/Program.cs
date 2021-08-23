using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
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

            while (true)
            {
                // 휴대폰 설정.
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 문지기에게 입장 문의.
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    // 송신.
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello World");
                    int sendByte = socket.Send(sendBuffer);

                    // 수신.
                    byte[] recvBuffer = new byte[1024];
                    int recvByte = socket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvByte);
                    Console.WriteLine($"[From Server] {recvData}");

                    // 퇴장.
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}
