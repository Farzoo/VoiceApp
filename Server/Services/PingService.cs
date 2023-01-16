using System;
using Core.Packets.Types;
using NetLib.Handlers.HandlerAttribute;
using NetLib.Packets;
using NetLib.Server;

namespace Server.Services;

public class PingService
{
    [PacketReceiver(typeof(PingPacket))]
    public void ReceivePing(BaseClient client, BasePacket basePacket)
    {
        Console.WriteLine("Ping received");
    }
}