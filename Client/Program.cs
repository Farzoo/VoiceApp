
using System.Diagnostics;
using System.Threading;
using Client.Services;
using Core;
using Core.Packets;
using Core.Packets.Serializer;
using Core.Packets.Types;
using NAudio.Wave;
using NetLib;
using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Server;

namespace Client
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            using (Process p = Process.GetCurrentProcess())
            {
                p.PriorityClass = ProcessPriorityClass.RealTime;
            }

            ClientSettings? settings = ClientSettings.GetSettings();
            
            // End if settings are null
            if (settings == null) return;

            Console.WriteLine($"{settings.ServerIp.Address}:{settings.ServerIp.Port}");
            
            IPacketMapper<ushort> mapper = new PacketMapper()
                .Register<PingPacket>()
                .Register<VoiceDataPacket>();
            
            IClient<BaseClient> client = new TcpClient(settings.ServerIp, new PacketSerializer(mapper));


            var handlerManager = new PacketServicesClientManager(client, new PacketSerializer(mapper), mapper)
                .RegisterPacketHandler(new ClientsVoiceHandler(new WaveFormat(48000, 16, 1), 20, 5));

            ClientWrapper clientWrapper = new ClientWrapper(client);
            VoiceRecorder voiceRecorder = new VoiceRecorder(clientWrapper);
            
            client.StartListening();

            /*WaitDisconnect waitDisconnect = new WaitDisconnect(client);
            waitDisconnect.Wait();*/
            

            while (client.IsConnected)
            {
                client.SendPacket(new PingPacket());
                Thread.Sleep(1000);
            }
        }

    }
}