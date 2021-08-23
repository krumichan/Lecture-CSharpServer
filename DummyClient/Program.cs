using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Packet packet = new Packet() { size = 4, packetId = 7 };

            // 송신.
            for (int i = 0; i < 5; ++i)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] sizeBuffer = BitConverter.GetBytes(packet.size);
                byte[] packetBuffer = BitConverter.GetBytes(packet.packetId);
                Array.Copy(sizeBuffer, 0, openSegment.Array, openSegment.Offset, sizeBuffer.Length);
                Array.Copy(packetBuffer, 0, openSegment.Array, openSegment.Offset + sizeBuffer.Length, packetBuffer.Length);
                ArraySegment<byte> realBuffer = SendBufferHelper.Close(packet.size);

                Send(realBuffer);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numberOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numberOfBytes}");
        }
    }

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

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {
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
