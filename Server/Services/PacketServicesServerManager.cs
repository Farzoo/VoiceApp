using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Server;
using NetLib.Services;

namespace Server.Services;


public class PacketServicesServerManager : PacketServicesManager<ushort, PacketServicesServerManager.ReceivedHandler, PacketServicesServerManager.SentHandler>
{
    public delegate void ReceivedHandler(BaseClient client, BasePacket basePacket);

    public delegate void SentHandler(BaseClient client, BasePacket basePacket);
    private IClientEvent ClientEventManager { get; }
    private IPacketSerializer Serializer { get; }
    
    public PacketServicesServerManager(IClientEvent clientEventManager, IPacketSerializer serializer, IPacketMapper<ushort> mapper) : base(mapper)
    {
        this.ClientEventManager = clientEventManager;
        this.ClientEventManager.OnClientConnected(this.OnConnected);
        this.ClientEventManager.OnClientDisconnected(this.OnDisconnected);
        this.Serializer = serializer;
    }
    
    private void OnConnected(BaseClient baseClient)
    {
        Console.WriteLine($"Client {baseClient.Id} connected");
        baseClient.RegisterOnReceive(this.OnReceive);
        baseClient.RegisterOnSend(this.OnSend);
    }

    private void OnDisconnected(BaseClient baseClient)
    {
        Console.WriteLine($"Client {baseClient.Id} disconnected");
        baseClient.UnregisterOnReceive(this.OnReceive);
        baseClient.UnregisterOnSend(this.OnSend);
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