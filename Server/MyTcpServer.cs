using System.Net;
using NetLib;
using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Server;
using Server.Services;

namespace Server;

public class MyTcpServer : TcpServer
{
    public MyTcpServer(IPacketSerializer serializer, IPacketMapper<ushort> mapper, IPEndPoint hostIp) : base(hostIp, serializer)
    {
        new PacketServicesServerManager(this, this.PacketSerializer, mapper)
            .RegisterPacketHandler(new VoiceDataBroadcastHandler(this))
            .RegisterPacketHandler(new PingService());
    }
}