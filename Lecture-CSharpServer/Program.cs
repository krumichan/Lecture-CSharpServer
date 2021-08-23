using System;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Lecture_CSharpServer
{
    class Packet
    {
        public int size;
        public int packetId;
    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] hpBuffer = BitConverter.GetBytes(packet.size);
            //byte[] attackBuffer = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(hpBuffer, 0, openSegment.Array, openSegment.Offset, hpBuffer.Length);
            //Array.Copy(attackBuffer, 0, openSegment.Array, openSegment.Offset + hpBuffer.Length, attackBuffer.Length);
            //ArraySegment<byte> realBuffer = SendBufferHelper.Close(hpBuffer.Length + attackBuffer.Length);

            //Send(realBuffer);
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"ReceivePacket - ID:{id}, Size:{size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnSend(int numberOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numberOfBytes}");
        }
    }

    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS ( Domain Name System )
            // Server Address   → Name Address
            // ex) 172.217.26.4 → www.google.com
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); // ipAddr: 식당 주소,  7777: 식당 문 위치.

            _listener.Init(endPoint, () => { return new GameSession(); });

            while (true)
            {
                ;
            }
        }
    }
}
