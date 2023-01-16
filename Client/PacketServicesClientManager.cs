using NetLib;
using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Server;
using NetLib.Services;

namespace Client;

public class PacketServicesClientManager : PacketServicesManager<ushort, PacketServicesClientManager.ReceivedHandler, PacketServicesClientManager.SentHandler>
{
    public delegate void ReceivedHandler(BaseClient client, BasePacket basePacket);

    public delegate void SentHandler(BaseClient client, BasePacket basePacket);
    
    private IPacketSerializer Serializer { get; }
    
    public PacketServicesClientManager(BaseClient client, IPacketSerializer serializer, IPacketMapper<ushort> mapper) : base(mapper)
    {
        client.RegisterOnReceive(this.OnReceive);
        client.RegisterOnSend(this.OnSend);
        
        this.Serializer = serializer;
    }
    
    private void OnReceive(BaseClient client, byte[] data)
    {
        BasePacket basePacket = this.Serializer.Deserialize(data);
        
        if(!this.Mapper.TryGetId(basePacket.GetType(), out var id)) return;
        
        this.ReceivedHandlers.TryGetValue(id, out var handlers);
        if(handlers is null) return;
        
        foreach (var packetReceivedHandler in handlers)
        {
            packetReceivedHandler.Invoke(client, basePacket);    
        }
    }

    private void OnSend(BaseClient client, BasePacket basePacket)
    {
        if(!this.Mapper.TryGetId(basePacket.GetType(), out var id)) return;
        
        this.SentHandlers.TryGetValue(id, out var handlers);
        
        if(handlers is null) return;

        foreach (var packetSentHandler in handlers)
        {
            packetSentHandler.Invoke(client, basePacket);    
        }
    }
}