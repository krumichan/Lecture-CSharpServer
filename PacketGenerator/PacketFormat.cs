using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return _instance; }} }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort byteCount = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + byteCount);
        byteCount += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + byteCount);
        byteCount += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
        {{
            action.Invoke(session, buffer);
        }}
    }}

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer); // packet deserialize.

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
        {{
            action.Invoke(session, packet);
        }}
    }}
}}
";
        // {0} 패킷 이름
        public static string managerRegisterFormat =
@"        _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);
";

        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using ServerCore;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        // {0} 패킷 이름.
        // {1} 멤버 변수.
        // {2} 멤버 변수 Read.
        // {3} 멤버 변수 Write.
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}

    public void Read(ArraySegment<byte> segment)
    {{
        ushort byteSize = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        byteSize += sizeof(ushort);
        byteSize += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);

        ushort byteSize = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        byteSize += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)PacketID.{0});
        byteSize += sizeof(ushort);

        {3}

        success &= BitConverter.TryWriteBytes(span, byteSize);
        if (success == false)
        {{
            return null;
        }}

        return SendBufferHelper.Close(byteSize);
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자 시작]
        // {1} 리스트 이름 [소문자 시작]
        // {2} 멤버 변수.
        // {3} 멤버 변수 Read.
        // {4} 멤버 변수 Write.
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> span, ref ushort byteSize)
    {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort byteSize)
    {{
        bool success = true;
        {4}

        return success;
    }}
}}

public List<{0}> {1}s = new List<{0}>();
";

        // {0} 변수 이름
        // {1} To~ 변수 형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(span.Slice(byteSize, span.Length - byteSize));
byteSize += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + byteSize];
byteSize += sizeof({1});";

        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Length = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
byteSize += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(byteSize, {0}Length));
byteSize += {0}Length;";

        // {0} 리스트 이름 [대문자 시작]
        // {1} 리스트 이름 [소문자 시작]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Length = BitConverter.ToUInt16(span.Slice(byteSize, span.Length - byteSize));
byteSize += sizeof(ushort);
for (int i = 0; i < {1}Length; ++i)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref byteSize);
    {1}s.Add({1});
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), this.{0});
byteSize += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"segment.Array[segment.Offset + byteSize] = (byte)this.{0};
byteSize += sizeof({1});";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + byteSize + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), {0}Length);
byteSize += sizeof(ushort);
byteSize += {0}Length;";

        // {0} 리스트 이름 [대문자 시작]
        // {1} 리스트 이름 [소문자 시작]
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(byteSize, span.Length - byteSize), (ushort)this.{1}s.Count);
byteSize += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Write(span, ref byteSize);
}}";
    }
}
