using Core;
using NetLib.Handlers;
using NetLib.Packets;
using NetLib.Server;
using NetLib.Services;

namespace Server.Services;


public class PacketServicesServerManager
    : PacketServicesManager<ushort, PacketServicesServerManager.ReceivedHandler, PacketServicesServerManager.SentHandler>
{
    public delegate void ReceivedHandler(ClientWrapper client, BasePacket basePacket);
    public delegate void SentHandler(ClientWrapper client, BasePacket basePacket);
    private IClientEvent ClientEventManager { get; }
    private IPacketSerializer Serializer { get; }
    
    private ISet<ClientWrapper> Clients { get; } = new HashSet<ClientWrapper>();
    
    public PacketServicesServerManager(IClientEvent clientEventManager, IPacketSerializer serializer, IPacketMapper<ushort> mapper) : base(mapper)
    {
        this.ClientEventManager = clientEventManager;
        this.ClientEventManager.OnClientConnected(this.OnConnected);
        
        this.Serializer = serializer;
    }
    
    private void OnConnected(IClient<BaseClient> client)
    {
        ClientWrapper clientWrapper = new ClientWrapper(client);
        clientWrapper.RegisterOnReceive(this.OnReceive);
        clientWrapper.RegisterOnSend(this.OnSend);

        clientWrapper.RegisterOnDisconnect(this.OnDisconnected);
        this.Clients.Add(clientWrapper);
    }

    private void OnDisconnected(IClient<ClientWrapper> client)
    {
        client.UnregisterOnReceive(this.OnReceive);
        client.UnregisterOnSend(this.OnSend);
    }

    private void OnReceive(ClientWrapper client, byte[] data)
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

    private void OnSend(ClientWrapper client, BasePacket basePacket)
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