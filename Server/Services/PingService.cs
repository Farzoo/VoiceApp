using System;
using Core;
using Core.Packets.Types;
using NetLib.Handlers.HandlerAttribute;
using NetLib.Packets;
using NetLib.Server;

namespace Server.Services;

public class PingService
{
    [PacketReceiver(typeof(PingPacket))]
    public void ReceivePing(ClientWrapper client, BasePacket basePacket)
    {
        Console.WriteLine("Ping received");
    }
}