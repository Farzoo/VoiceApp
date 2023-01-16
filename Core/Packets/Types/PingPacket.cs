using NetLib.Packets;
using ProtoBuf;

namespace Core.Packets.Types;

[ProtoContract]
[PacketInfo(PacketType.Ping)]
public class PingPacket : BasePacket
{
    public ushort Id => (ushort)PacketType.Ping;
    
    public PingPacket() { }
}