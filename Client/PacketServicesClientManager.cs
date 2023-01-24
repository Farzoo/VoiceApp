using NetLib.Packets;
using NetLib.Server;
using NetLib.Services;

namespace Client;

public class PacketServicesClientManager : PacketServicesManager<ushort, PacketServicesClientManager.ReceivedHandler, PacketServicesClientManager.SentHandler>
{
    public delegate void ReceivedHandler(IClient<BaseClient> client, BasePacket basePacket);

    public delegate void SentHandler(IClient<BaseClient> client, BasePacket basePacket);
    private IPacketSerializer Serializer { get; }
    
    private IClient<BaseClient> Client { get; }
    
    public PacketServicesClientManager(IClient<BaseClient> client, IPacketSerializer serializer, IPacketMapper<ushort> mapper) : base(mapper)
    {
        this.Client = client;
        
        this.Client.RegisterOnReceive(this.OnReceive);
        this.Client.RegisterOnSend(this.OnSend);
        
        this.Serializer = serializer;
    }
    
    private void OnReceive(IClient<BaseClient> client, byte[] data)
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

    private void OnSend(IClient<BaseClient> client, BasePacket basePacket)
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