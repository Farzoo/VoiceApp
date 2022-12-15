using System;
using System.Threading;
using NAudio.Wave;
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
        
        public static void Main(string[] args)
        {
            ClientSettings settings = ClientSettings.GetSettings();
        
            PacketMapper mapper = new PacketMapper();
            mapper.Register<LoginPacket>();
            mapper.Register<PingPacket>();
            mapper.Register<VoiceDataPacket>();

            ClientsHandler clientsHandler = new ClientsHandler();
            PacketHandlerManager handlerManager = new PacketHandlerManager(clientsHandler, new PacketSerializer(mapper));
            handlerManager.RegisterPacketReceivedHandler(new TimeoutHandler());
            BaseClient client = CreateTcpClient(settings, new PacketSerializer(mapper));
            clientsHandler.ConnectClient(client);
            //BaseClient client = CreateTcpClient(settings, mapper);
            client.SendPacket(new LoginPacket("test", "test"));
            _client = client;
            Thread.Sleep(1000);
            
            //WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(0);
            Console.WriteLine("Now recording...");
            WaveInEvent waveSource = new WaveInEvent();
            //waveSource.DeviceNumber = 0;
            waveSource.WaveFormat = new WaveFormat(44100, 1);

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            
            waveSource.StartRecording();

            while (true)
            {
                // Send microphone audio 
                
            }
        }

        private static BaseClient _client;
        
        static void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Send microphone audio and play it
            Console.WriteLine($"Sending audio {e.BytesRecorded}");
            _client.SendPacket(new VoiceDataPacket(e.Buffer));
            // Play audio
            /*WaveOut waveOut = new WaveOut();
            waveOut.Init(new RawSourceWaveStream(e.Buffer, 0, e.BytesRecorded, new WaveFormat(44100, 1)));
            waveOut.Play();*/
        }

    }
}