using System.Collections.Concurrent;
using Core.Audio;
using Core.Packets;
using Core.Packets.Types;
using NAudio.Wave;
using NetLib.Handlers.HandlerAttribute;
using NetLib.Packets;
using NetLib.Server;

namespace Client.Services;

public class ClientsVoiceHandler
{
    private IDictionary<Guid, ClientVoiceOut> Clients { get; } = new ConcurrentDictionary<Guid, ClientVoiceOut>();
    private ContinuousMixingSampleProvider Mixer { get; }
    private WaveFormat DefaultFormat { get; } 
    private int DefaultLatency { get; }
    private int DefaultDurationMultiplier { get; }
    
    private WaveOutEvent Output { get; }

    public ClientsVoiceHandler(WaveFormat defaultFormat, int defaultLatency, int defaultDurationMultiplier)
    {
        this.DefaultFormat = defaultFormat;
        this.DefaultLatency = defaultLatency;
        this.DefaultDurationMultiplier = defaultDurationMultiplier;
        this.Mixer = new ContinuousMixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(this.DefaultFormat.SampleRate, this.DefaultFormat.Channels));
        this.Output = new WaveOutEvent();
        this.Output.Init(this.Mixer);
    }
    
    [PacketReceiver(typeof(VoiceDataPacket))]
    public void OnVoiceReceived(IClient<BaseClient> client, BasePacket basePacket)
    {
        if(basePacket is not VoiceDataPacket voicePacket) return;
        
        lock(this.Clients)
        {
            if (!this.Clients.ContainsKey(voicePacket.EntityId))
            {
                this.ConnectClient(voicePacket.EntityId);
            }
        }
        
        Console.WriteLine($"Received voice packet from {voicePacket.EntityId}");

        this.Clients[voicePacket.EntityId].PlayReceivedVoice(voicePacket);
        this.Output.Play();
    }
    
    public void DisconnectClient(Guid clientId)
    {
        lock(this.Clients)
        {
            if (this.Clients.ContainsKey(clientId))
            {
                this.Clients[clientId].Stop();
                this.Clients.Remove(clientId);
            }
        }
    }

    private void ConnectClient(Guid clientId)
    {
        lock(this.Clients)
        {
            this.Clients.Add
            (
                clientId,
                new ClientVoiceOut(this.Mixer, clientId, this.DefaultFormat, this.DefaultLatency, this.DefaultDurationMultiplier)
            );
        }
    }
}