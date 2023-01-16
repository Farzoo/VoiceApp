using NetLib.Packets;

namespace Core.Packets.Types;

[PacketInfo(PacketType.Unknown)]
public class UnknownPacket : BasePacket
{
    public ushort Id => (ushort)PacketType.Unknown;
}