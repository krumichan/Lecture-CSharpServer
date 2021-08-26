using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    C_PlayerInfoReq = 1,
	S_Test = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class C_PlayerInfoReq : IPacket
{
    public byte testByte;
	public long playerId;
	public string name;
	
	public class Skill
	{
	    public int id;
		public short level;
		public float duration;
		
		public class Attribute
		{
		    public int att;
		
		    public void Read(ReadOnlySpan<byte> span, ref ushort byteSize)
		    {
		        this.att = BitConverter.ToInt32(span.Slice(byteSize, span.Length - byteSize));
				byteSize += sizeof(int);
		    }
		
		    public bool Write(Span<byte> span, ref ushort byteSize)
		    {
		        bool success = true;
		        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.att);
				byteSize += sizeof(int);
		
		        return success;
		    }
		}
		
		public List<Attribute> attributes = new List<Attribute>();
		
	
	    public void Read(ReadOnlySpan<byte> span, ref ushort byteSize)
	    {
	        this.id = BitConverter.ToInt32(span.Slice(byteSize, span.Length - byteSize));
			byteSize += sizeof(int);
			this.level = BitConverter.ToInt16(span.Slice(byteSize, span.Length - byteSize));
			byteSize += sizeof(short);
			this.duration = BitConverter.ToSingle(span.Slice(byteSize, span.Length - byteSize));
			byteSize += sizeof(float);
			this.attributes.Clear();
			ushort attributeLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
			byteSize += sizeof(ushort);
			for (int i = 0; i < attributeLength; ++i)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(span, ref byteSize);
			    attributes.Add(attribute);
			}
	    }
	
	    public bool Write(Span<byte> span, ref ushort byteSize)
	    {
	        bool success = true;
	        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.id);
			byteSize += sizeof(int);
			success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.level);
			byteSize += sizeof(short);
			success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.duration);
			byteSize += sizeof(float);
			success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)this.attributes.Count);
			byteSize += sizeof(ushort);
			foreach (Attribute attribute in this.attributes)
			{
			    success &= attribute.Write(span, ref byteSize);
			}
	
	        return success;
	    }
	}
	
	public List<Skill> skills = new List<Skill>();
	

    public ushort Protocol { get { return (ushort)PacketID.C_PlayerInfoReq; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.testByte = (byte)segment.Array[segment.Offset + byteSize];
		byteSize += sizeof(byte);
		this.playerId = BitConverter.ToInt64(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(long);
		ushort nameLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(span.Slice(byteSize, nameLength));
		byteSize += nameLength;
		this.skills.Clear();
		ushort skillLength = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(ushort);
		for (int i = 0; i < skillLength; ++i)
		{
		    Skill skill = new Skill();
		    skill.Read(span, ref byteSize);
		    skills.Add(skill);
		}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort byteSize = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        byteSize += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)PacketID.C_PlayerInfoReq);
        byteSize += sizeof(ushort);

        segment.Array[segment.Offset + byteSize] = (byte)this.testByte;
		byteSize += sizeof(byte);
		success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.playerId);
		byteSize += sizeof(long);
		ushort nameLength = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + byteSize + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), nameLength);
		byteSize += sizeof(ushort);
		byteSize += nameLength;
		success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)this.skills.Count);
		byteSize += sizeof(ushort);
		foreach (Skill skill in this.skills)
		{
		    success &= skill.Write(span, ref byteSize);
		}

        success &= BitConverter.TryWriteBytes(span, byteSize);
        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(byteSize);
    }
}

class S_Test : IPacket
{
    public int testInt;

    public ushort Protocol { get { return (ushort)PacketID.S_Test; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.testInt = BitConverter.ToInt32(span.Slice(byteSize, span.Length - byteSize));
		byteSize += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort byteSize = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        byteSize += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)PacketID.S_Test);
        byteSize += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.testInt);
		byteSize += sizeof(int);

        success &= BitConverter.TryWriteBytes(span, byteSize);
        if (success == false)
        {
            return null;
        }

        return SendBufferHelper.Close(byteSize);
    }
}

