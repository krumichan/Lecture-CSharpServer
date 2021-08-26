using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Lecture_CSharpServer
{
	class ClientSession : PacketSession
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
            PacketManager.Instance.OnRecvPacket(this, buffer);
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
}
