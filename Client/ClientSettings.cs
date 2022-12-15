using System;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace ClientChatApp;

public class ClientSettings
{
    private IPEndPoint _serverIp;
    public IPEndPoint ServerIp
    {
        get => this._serverIp;
        set => this._serverIp = value;
    }

    private String _ip;

    public String Ip
    {
        get => this._ip;
        set
        {
            IPAddress temp;
            if (IPAddress.TryParse(value, out temp))
            {
                this.ServerIp.Address = temp;
                this._ip = value;
            }
        }
    }

    private int _port;

    public int Port
    {
        get => this._port;
        set
        {
            if (value >= IPEndPoint.MinPort && value <= IPEndPoint.MaxPort)
            {
                this._port = value;
                this.ServerIp.Port = value;
            }
        }
    }

    public ClientSettings()
    {
        this.ServerIp = new IPEndPoint(IPAddress.Any, 0);
    }
    
    public static ClientSettings GetSettings()
    {
        return ClientSettings.GetSettings("appsettings.json");
    }

    public static ClientSettings GetSettings(String settingsPath)
    {
        ClientSettings settings = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(settingsPath)
            .Build()
            .GetSection(nameof(ClientSettings))
            .Get<ClientSettings>();
        return settings;
    }
}