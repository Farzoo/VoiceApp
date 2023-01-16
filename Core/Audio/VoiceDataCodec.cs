using System.Runtime.InteropServices;
using Concentus.Enums;
using Concentus.Structs;
using Core.Packets;
using Core.Packets.Types;
using NAudio.Wave;

namespace Core.Audio;

public class VoiceDataCodec
{
    private WaveFormat WaveFormat { get; }
    public int FrameSize { get; }
    public int Latency { get; }
    public int DurationMultiplier { get; }
    
    private short[] PcmBuffer { get; }
    private byte[] OpusBuffer { get; }
    
    private OpusDecoder Decoder { get; }
    private OpusEncoder Encoder { get; }

    public VoiceDataCodec(WaveFormat waveFormat, int latency, int durationMultiplier)
    {
        this.WaveFormat = waveFormat;
        this.Latency = latency;
        this.DurationMultiplier = durationMultiplier;
        
        this.FrameSize = this.WaveFormat.ConvertLatencyToByteSize(this.Latency);
        
        this.PcmBuffer = new short[this.FrameSize * this.WaveFormat.ConvertLatencyToByteSize(this.DurationMultiplier)];
        this.OpusBuffer = new byte[this.FrameSize * this.WaveFormat.ConvertLatencyToByteSize(this.DurationMultiplier)];

        this.Decoder = OpusDecoder.Create(this.WaveFormat.SampleRate, this.WaveFormat.Channels);
        this.Encoder = OpusEncoder.Create(this.WaveFormat.SampleRate, this.WaveFormat.Channels, OpusApplication.OPUS_APPLICATION_VOIP);
    }

    /**
     * Returns a span of the encoded voice data. The span is a view on the internal buffer, so it is only valid until the next call to this method.
     * No memory allocation is done for each call.
     */
    public Span<byte> DecodeVoiceData(VoiceDataPacket voiceDataPacket)
    {
        int pcmOffset = 0;
        int opusOffset = 0;
        for (int i = 0; i < voiceDataPacket.DataOffsets.Length; i++)
        {
            pcmOffset += this.Decoder.Decode(voiceDataPacket.Data, opusOffset, voiceDataPacket.DataOffsets[i],
                this.PcmBuffer, pcmOffset, this.FrameSize);
            opusOffset += voiceDataPacket.DataOffsets[i];
        }
        return MemoryMarshal.Cast<short, byte>(this.PcmBuffer.AsSpan(0, pcmOffset));
    }
    
    /**
     * Returns a span of the encoded voice data. The span is a view on the internal buffer, so it is only valid until the next call to this method.
     * No memory allocation is done for each call.
     */
    public Span<byte> EncodeVoiceData(Span<byte> pcm, out int[] offsets)
    {
        short[] data = MemoryMarshal.Cast<byte, short>(pcm).ToArray();
        int offset = 0;
        int pcmSliceSize = this.FrameSize / 2; // We need to divide by 2 because we are working with shorts
        int[] bufferOffsets = new int[this.DurationMultiplier];
        
        Array.Clear(this.OpusBuffer, 0, this.OpusBuffer.Length);
        Console.WriteLine(pcmSliceSize);
        
        for (int i = 0; i < this.DurationMultiplier; i++)
        {
            bufferOffsets[i] = Encoder.Encode(
                data, 
                i * pcmSliceSize, 
                pcmSliceSize, this.OpusBuffer,
                offset, 
                this.OpusBuffer.Length - (i * pcmSliceSize)
            );
            offset += bufferOffsets[i];
        }

        offsets = bufferOffsets;
        return this.OpusBuffer.AsSpan(0, offset);
    }
}
