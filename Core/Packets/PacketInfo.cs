using Core.Packets;
using Core.Packets.Types;
using NetLib.Handlers.HandlerAttribute;

namespace Core;

public class PacketInfo : PacketInfoBase<ushort>
{
    public PacketInfo(PacketType type) : base((ushort) type)
    {
    }
}