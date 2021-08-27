using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class S_BroadcastEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastEnterGame; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(int);
		this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.S_BroadcastEnterGame), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + byteSize, sizeof(int));
		byteSize += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

public class C_LeaveGame : IPacket
{
    

    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.C_LeaveGame), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.S_BroadcastLeaveGame), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + byteSize, sizeof(int));
		byteSize += sizeof(int);

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

public class S_PlayerList : IPacket
{
    
	public class Player
	{
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	    public void Read(ArraySegment<byte> segment, ref ushort byteSize)
	    {
	        this.isSelf = BitConverter.ToBoolean(segment.Array, segment.Offset + byteSize);
			byteSize += sizeof(bool);
			this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + byteSize);
			byteSize += sizeof(int);
			this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
			byteSize += sizeof(float);
			this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
			byteSize += sizeof(float);
			this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
			byteSize += sizeof(float);
	    }
	
	    public bool Write(ArraySegment<byte> segment, ref ushort byteSize)
	    {
	        bool success = true;
	        Array.Copy(BitConverter.GetBytes(this.isSelf), 0, segment.Array, segment.Offset + byteSize, sizeof(bool));
			byteSize += sizeof(bool);
			Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + byteSize, sizeof(int));
			byteSize += sizeof(int);
			Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
			byteSize += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
			byteSize += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
			byteSize += sizeof(float);
	
	        return success;
	    }
	}
	
	public List<Player> players = new List<Player>();
	

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.players.Clear();
		ushort playerLength = BitConverter.ToUInt16(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(ushort);
		for (int i = 0; i < playerLength; ++i)
		{
		    Player player = new Player();
		    player.Read(segment, ref byteSize);
		    players.Add(player);
		}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.S_PlayerList), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)this.players.Count), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
		byteSize += sizeof(ushort);
		foreach (Player player in this.players)
		{
		    player.Write(segment, ref byteSize);
		}

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

public class C_Move : IPacket
{
    public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.C_Move), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort byteSize = 0;
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(int);
		this.posX = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posY = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
		this.posZ = BitConverter.ToSingle(segment.Array, segment.Offset + byteSize);
		byteSize += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort byteSize = 0;

        byteSize += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((short)PacketID.S_BroadcastMove), 0, segment.Array, segment.Offset + byteSize, sizeof(ushort));
        byteSize += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, segment.Array, segment.Offset + byteSize, sizeof(int));
		byteSize += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, segment.Array, segment.Offset + byteSize, sizeof(float));
		byteSize += sizeof(float);

        Array.Copy(BitConverter.GetBytes(byteSize), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(byteSize);
    }
}

