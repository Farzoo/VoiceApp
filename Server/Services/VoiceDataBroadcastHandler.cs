﻿using Core.Packets;
using Core.Packets.Types;
using NetLib.Handlers.HandlerAttribute;
using NetLib.Packets;
using NetLib.Server;

namespace Server.Services;

public class VoiceDataBroadcastHandler
{ 
    private BaseServer Server { get; }
    public VoiceDataBroadcastHandler(BaseServer server)
    {
        this.Server = server;
    }

    [PacketReceiver(typeof(VoiceDataPacket))]
    public void BroadcastVoice(BaseClient baseClient, BasePacket basePacket)
    {
        if (basePacket is not VoiceDataPacket voiceDataPacket) return;

        voiceDataPacket.EntityId = baseClient.Id;
        
        this.Server.Broadcast(voiceDataPacket, baseClient);
    }
}