using System.Net;
using NetLib.Handlers;
using NetLib.Packets;

namespace NetLib.Server;

public class MyUdpServer : UdpServer
{

    public MyUdpServer(IPacketSerializer serializer, IPacketMapper<ushort> mapper, IPEndPoint hostIp) : base(hostIp, serializer)
    {
    }
}