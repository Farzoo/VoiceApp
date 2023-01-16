﻿using NetLib.Packets;
using ProtoBuf;

namespace Core.Packets.Types;

[ProtoContract]
[PacketInfo(PacketType.VoiceData)]
public class VoiceDataPacket : BasePacket
{
    public ushort Id => (ushort) PacketType.VoiceData;

    [ProtoMember(1)]
    public byte[] Data { get; }
    
    [ProtoMember(2)]
    public int[] DataOffsets { get; }
    
    [ProtoMember(3)]
    public TimeSpan Time { get; }
    
    [ProtoMember(4)]
    public ulong Sequence { get; }
    
    [ProtoMember(5)]
    public Guid EntityId { get; set; }
    
    public VoiceDataPacket(byte[] data, int[] dataOffsets, TimeSpan time, uint sequence, Guid entityId)
    {
        this.Data = data;
        this.DataOffsets = dataOffsets;
        this.Time = time;
        this.Sequence = sequence;
        this.EntityId = entityId;
    }
    

    public VoiceDataPacket()
    {
        this.EntityId = Guid.Empty;
        this.Data = Array.Empty<byte>();
        this.DataOffsets = Array.Empty<int>();
        this.Time = TimeSpan.Zero;
        this.Sequence = 0;
    }
}