using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace DummyClient
{
	class ServerSession : PacketSession
    {
        /*// unsafe : C++ 마냥 pointer 사용 가능.
        static unsafe void ToByte(byte[] array, int offset, ulong value)
        {
            fixed (byte* ptr = &array[offset])
            {
                *(ulong*)ptr = value;
            }
        }*/

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numberOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numberOfBytes}");
        }
    }
}
