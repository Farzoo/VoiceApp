using System;
using System.Threading;
using NetLib;
using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Packets.Client;
using NetLib.Packets.Shared;
using NetLib.Server;

namespace Client
{
    internal class Program
    {
        public static BaseClient CreateUdpClient(ClientSettings settings, IPacketSerializer serializer)
        {
            BaseClient client = new UdpClient(listenIpEndPoint: settings.ServerIp, sendIpEndPoint: settings.ServerIp, serializer);
            client.StartListening();
            return client;
        }
        
        public static BaseClient CreateTcpClient(ClientSettings settings, IPacketSerializer serializer)
        {
            BaseClient client = new TcpClient(settings.ServerIp, serializer);
            client.StartListening();
            return client;
        }
        
        public static PacketHandlerManager CreatePacketHandlerManager(IClientEvent clientEvent, PacketSerializer serializer)
        {
            PacketHandlerManager packetHandlerManager = new PacketHandlerManager(clientEvent, serializer);
            packetHandlerManager.RegisterPacketReceivedHandler(new VoiceDataClientHandler());
            packetHandlerManager.RegisterPacketReceivedHandler(new TimeoutHandler());
            
            return packetHandlerManager;
        }
        
        public static void Main(string[] args)
        {
            ClientSettings settings = ClientSettings.GetSettings();
            
            Console.WriteLine($"{settings.ServerIp.Address}:{settings.ServerIp.Port}");
        
            PacketMapper mapper = new PacketMapper();
            mapper.Register<LoginPacket>();
            mapper.Register<PingPacket>();
            mapper.Register<VoiceDataPacket>();

            ClientsHandler clientsHandler = new ClientsHandler();
            PacketHandlerManager handlerManager = CreatePacketHandlerManager(clientsHandler, new PacketSerializer(mapper));
            
            BaseClient client = CreateTcpClient(settings, new PacketSerializer(mapper));
            clientsHandler.ConnectClient(client);
            
            VoiceDataEventHandler voiceDataEventHandler = new VoiceDataEventHandler(client);
            
            client.SendPacket(new LoginPacket("test", "test"));

            while (client.IsConnected)
            {
                Thread.Sleep(10000);
            }
        }
    }
}