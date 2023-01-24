using NetLib.Packets;
using NetLib.Server;

namespace Core;

public sealed class ClientWrapper : IClient<ClientWrapper>
{
    private event IClient<ClientWrapper>.OnReceiveHandler<ClientWrapper>? OnReceive;
    private event IClient<ClientWrapper>.OnSendHandler<ClientWrapper>? OnSend;
    private event IClient<ClientWrapper>.OnDisconnectHandler<ClientWrapper>? OnDisconnect;
    
    public IClient<BaseClient> InnerClient { get; }
    public bool IsConnected => this.InnerClient.IsConnected;

    public bool IsListening => this.InnerClient.IsListening;
    
    public Guid Id { get; } = Guid.NewGuid();

    public ClientWrapper(IClient<BaseClient> innerClient)
    {
        this.InnerClient = innerClient;
        this.InnerClient.RegisterOnDisconnect(this.InvokeOnDisconnect);
        this.InnerClient.RegisterOnReceive(this.InvokeOnReceive);
        this.InnerClient.RegisterOnSend(this.InvokeOnSend);
    }

    public void Connect()
    {
        this.InnerClient.Connect();
    }

    public void Disconnect()
    {
        this.InnerClient.Disconnect();
    }

    public void StartListening()
    {
        this.InnerClient.StartListening();
    }

    public void SendPacket<T>(T packet) where T : BasePacket
    {
        this.InnerClient.SendPacket(packet);
    }

    public void RegisterOnReceive(IClient<ClientWrapper>.OnReceiveHandler<ClientWrapper> handler)
    {
        this.OnReceive += handler;
    }
    
    public void RegisterOnSend(IClient<ClientWrapper>.OnSendHandler<ClientWrapper>? handler)
    {
        this.OnSend += handler;
    }
    
    public void UnregisterOnReceive(IClient<ClientWrapper>.OnReceiveHandler<ClientWrapper> handler)
    {
        this.OnReceive -= handler;
    }
    
    public void UnregisterOnSend(IClient<ClientWrapper>.OnSendHandler<ClientWrapper>? handler)
    {
        this.OnSend -= handler;
    }
    
    public void RegisterOnDisconnect(IClient<ClientWrapper>.OnDisconnectHandler<ClientWrapper> handler)
    {
        this.OnDisconnect += handler;
    }
    
    public void UnregisterOnDisconnect(IClient<ClientWrapper>.OnDisconnectHandler<ClientWrapper> handler)
    {
        this.OnDisconnect -= handler;
    }

    private void InvokeOnSend(IClient<BaseClient> client, BasePacket basePacket)
    {
        this.OnSend?.Invoke(this, basePacket);
    }

    private void InvokeOnDisconnect(IClient<BaseClient> client)
    {
        this.InnerClient.UnregisterOnDisconnect(this.InvokeOnDisconnect);
        this.InnerClient.UnregisterOnReceive(this.InvokeOnReceive);
        this.InnerClient.UnregisterOnSend(this.InvokeOnSend);
        
        this.OnDisconnect?.Invoke(this);
    }

    private void InvokeOnReceive(IClient<BaseClient> client, byte[] data)
    {
        this.OnReceive?.Invoke(this, data);
    }
}