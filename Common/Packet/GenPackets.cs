using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        ushort chatLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(byteSize, chatLength));
		byteSize += chatLength;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort byteSize = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        byteSize += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)PacketID.C_Chat);
        byteSize += sizeof(ushort);

        ushort chatLength = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + byteSize + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), chatLength);
		byteSize += sizeof(ushort);
		byteSize += chatLength;

        success &= BitConverter.TryWriteBytes(span, byteSize);
        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(byteSize);
    }
}

class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(int);
		ushort chatLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(span.Slice(byteSize, chatLength));
		byteSize += chatLength;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort byteSize = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        byteSize += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)PacketID.S_Chat);
        byteSize += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.playerId);
		byteSize += sizeof(int);
		ushort chatLength = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + byteSize + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), chatLength);
		byteSize += sizeof(ushort);
		byteSize += chatLength;

        success &= BitConverter.TryWriteBytes(span, byteSize);
        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(byteSize);
    }
}

