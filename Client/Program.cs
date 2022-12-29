using System;
using System.Diagnostics;
using System.Threading;
using NAudio.Wave;
using NetLib.Handlers;
using NetLib.Handlers.Client;
using NetLib.Packets;
using NetLib.Packets.Client;
using NetLib.Packets.Shared;
using NetLib.Server;

namespace Client
{
    internal class Program
    {
        public static IPacketHandlerManager CreatePacketHandlerManager(BaseClient client, IPacketSerializer serializer)
        {
            IPacketHandlerManager packetHandlerServerManager = new PacketHandlerClientManager(client, serializer);
            packetHandlerServerManager.RegisterPacketReceivedHandler(new ClientsVoiceHandler(new WaveFormat(48000, 16, 1), 20, 5));
            packetHandlerServerManager.RegisterPacketReceivedHandler(new TimeoutHandler());
            return packetHandlerServerManager;
        }
        
        public static void Main(string[] args)
        {
            using (Process p = Process.GetCurrentProcess())
            {
                p.PriorityClass = ProcessPriorityClass.RealTime;
            }
            
            ClientSettings settings = ClientSettings.GetSettings();
            
            Console.WriteLine($"{settings.ServerIp.Address}:{settings.ServerIp.Port}");
        
            IPacketMapper mapper = new PacketMapper()
                .Register<LoginPacket>()
                .Register<PingPacket>()
                .Register<VoiceDataPacket>();

            BaseClient client = new TcpClient(settings.ServerIp, new PacketSerializer(mapper));
            
            IPacketHandlerManager handlerServerManager = CreatePacketHandlerManager(client, new PacketSerializer(mapper));

            VoiceRecorder voiceRecorder = new VoiceRecorder(client);

            client.StartListening();
            
            client.SendPacket(new LoginPacket("test", "test"));

            var waitDisconnect = new WaitDisconnect(client);
        }
    }
}