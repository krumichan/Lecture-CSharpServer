using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DummyClient
{
    abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> span, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), duration);
                count += sizeof(float);

                return success;
            }

            public void Read(ReadOnlySpan<byte> span, ref ushort count)
            {
                id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
                count += sizeof(float);
            }
        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort byteSize = 0;
            bool success = true;

            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            byteSize += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.packetId); // -byteCount는 Offset이 움직인 만큼 공간이 줄기 때문.
            byteSize += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.playerId);
            byteSize += sizeof(long);

            // string
            // nameLength를 담을 ushort 크기만큼 미리 비워둔다.
            ushort nameLength = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + byteSize + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), nameLength);
            byteSize += sizeof(ushort);
            byteSize += nameLength;

            // skill list
            success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)skills.Count);
            byteSize += sizeof(ushort);
            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(span, ref byteSize);
            }

            // packet의 최종적인 크기를 모르기 때문에,
            // 모든 byte 계산이 끝나고 후에 여기서 넣어준다.
            success &= BitConverter.TryWriteBytes(span, byteSize);

            if (success == false)
            {
                return null;
            }

            return SendBufferHelper.Close(byteSize);
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort byteSize = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            byteSize += sizeof(ushort);
            byteSize += sizeof(ushort);
            this.playerId = BitConverter.ToInt64(span.Slice(byteSize, span.Length - byteSize));
            byteSize += sizeof(long);

            // string
            ushort nameLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
            byteSize += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(span.Slice(byteSize, nameLength));
            byteSize += nameLength;

            // skill list
            skills.Clear();
            ushort skillLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
            byteSize += sizeof(ushort);
            for (int i = 0; i < skillLength; ++i)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(span, ref byteSize);
                skills.Add(skill);
            }
        }
    }

    /*class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }*/

    public enum PacketID
    {
        PlayerInfoReq = 1
        , PlayerInfoOk = 2
    }

    class ServerSession : Session
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

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name="hito" };
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 201, level = 2, duration = 4.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 301, level = 3, duration = 5.0f });
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 401, level = 4, duration = 6.0f });

            // 송신.
            //for (int i = 0; i < 5; ++i)
            {
                ArraySegment<byte> s = packet.Write();
                if (s != null)
                {
                    Send(s);
                }
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
}
