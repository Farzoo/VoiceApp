using Core.Audio;
using Core.Packets;
using Core.Packets.Types;
using NAudio.Wave;

namespace Client.Services;

public class ClientVoiceOut
{
    private readonly Guid _clientId;
    private WaveFormat Format { get; }
    private VolumeWaveProvider16 VolumeWaveProvider { get; }
    private ContinuousMixingSampleProvider Mixer { get; }
    private JitterBuffer JitterBuffer { get; }
    
    private ISampleProvider Handle { get; }

    private VoiceDataCodec VoiceHandler { get; }

    public ClientVoiceOut(ContinuousMixingSampleProvider mixer, Guid clientId, WaveFormat waveFormat, int latency, int durationMultiplier, float volume = 1.0f)
    {
        this._clientId = clientId;

        this.Format = waveFormat;
        
        this.JitterBuffer = new JitterBuffer(this.Format, latency * durationMultiplier * 4);

        this.VolumeWaveProvider = new VolumeWaveProvider16(this.JitterBuffer)
        {
            Volume = volume
        };

        this.Handle = SampleProviderConverters.ConvertWaveProviderIntoSampleProvider(this.VolumeWaveProvider);

        this.VoiceHandler = new VoiceDataCodec(this.Format, latency, durationMultiplier);
        
        this.Mixer = mixer;
        this.Mixer.AddMixerInput(this.Handle);
    }
    
    public void PlayReceivedVoice(VoiceDataPacket packet)
    {
        if(packet.EntityId != this._clientId) return;
        
        Span<byte> decoded = this.VoiceHandler.DecodeVoiceData(packet);
        
        this.JitterBuffer.AddSamples(decoded.ToArray(), 0, decoded.Length, packet.Sequence, packet.Time);
    }

    public void Stop()
    {
        this.Mixer.RemoveMixerInput(this.Handle);
    }
    
}