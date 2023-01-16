using System.Net;
using System.Net.Sockets;
using Core.Packets;
using Core.Packets.Serializer;
using Core.Packets.Types;
using NetLib.Packets;
using NetLib.Server;

namespace Server
{
    internal class Program
    {
        public static void LaunchTcpServer(IPacketSerializer serializer, IPacketMapper<ushort> mapper, IPEndPoint ipEndPoint)
        {
            MyTcpServer chatTcpServer = new MyTcpServer(
                serializer,
                mapper,
                ipEndPoint
            );
            chatTcpServer.Start();
        }

        public static void LaunchUdpServer(IPacketSerializer serializer, IPacketMapper<ushort> mapper, IPEndPoint ipEndPoint)
        {
            UdpServer chatUdpServer = new MyUdpServer(
                serializer,
                mapper,
                ipEndPoint
            );
            
            chatUdpServer.Start();
        }

        public static void Main(string[] args)
        {

            IPacketMapper<ushort> mapper = new PacketMapper()
                .Register<PingPacket>()
                .Register<VoiceDataPacket>();

            IPacketSerializer serializer = new PacketSerializer(mapper);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 7777);

            //LaunchTcpServer(serializer, ipEndPoint);
            LaunchTcpServer(serializer, mapper, ipEndPoint);
        }
    }
}