using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetLib.Packets;
using NetLib.Packets.Client;
using NetLib.Packets.Shared;
using NetLib.Server;
using TcpClient = System.Net.Sockets.TcpClient;

namespace Server
{
    internal class Program
    {
        public static void LaunchTcpServer(IPacketMapper mapper, IPEndPoint ipEndPoint)
        {
            MyTcpServer chatTcpServer = new MyTcpServer(
                mapper,
                ipEndPoint
            );
            chatTcpServer.Start();
        }

        public static void LaunchUdpServer(IPacketMapper mapper, IPEndPoint ipEndPoint)
        {
            UdpServer chatUdpServer = new MyUdpServer(
                mapper,
                ipEndPoint
            );
            
            chatUdpServer.Start();
        }

        public static void Main(string[] args)
        {
            
            IPacketMapper mapper = new PacketMapper();
            mapper.Register<LoginPacket>();
            mapper.Register<PingPacket>();
            mapper.Register<VoiceDataPacket>();
            
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 7777);
            
            //LaunchTcpServer(mapper, ipEndPoint);
            LaunchTcpServer(mapper, ipEndPoint);
        }
    }
}