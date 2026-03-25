using System.Collections;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using THGameClient;
using UnityEngine;

public class NetworkManager : IManager
{
    private InputManager InputManager
    {
        get
        {
            if (_inputManager == null)
            {
                _inputManager = GameManager.GetManager<InputManager>();
            }
            return _inputManager;
        }
    }
    private InputManager _inputManager;

    private GameClient _client;
    public long BytesReceivedPerSecond { get => _client.BytesReceivedPerSecond; }
    public long BytesSentPerSecond { get => _client.BytesSentPerSecond; }
    public long PacketLossPercent { get => _client.PacketLossPercent; }

    public int Ping { get => _client.Ping; }
    public NetStatistics NetworkStats { get => _client.NetworkStats; }

    internal event GameClient.ConnectionEvent OnClientConnected;

    internal event GameClient.DisconnectionEvent OnClientDisconnected;

    public void Connect(string username, string password)
    {
        _client.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.1"), 1234), 
            username, password, new THNetworkLibrary.Tools.DebugNetworkSettings() 
            {
                SimulatedLatency = 50,
                SimulatedLatencyRange = 10,
                SimulatedPacketLossChance = 1 
            });
    }

    public void Disconnect()
    {
        if (_client != null)
        {
            _client.Disconnect();
        }
    }

    public override ManagerInitializeErrorCode Initialize()
    {
        _client = new GameClient(InputManager, new ClientNetworkObjectManager());
        _client.OnConnectedToServer += () => { OnClientConnected?.Invoke(); };
        _client.OnDisconnectedFromServer += () => { OnClientDisconnected?.Invoke(); };

        return ManagerInitializeErrorCode.NONE;
    }

    public override void Update(float delta)
    {
        if (_client.ConnectionState == ServerConnectionState.Connected || _client.ConnectionState == ServerConnectionState.Connecting)
        {
            _client.RunClient();    
        }
    }

    public override void OnDestroy()
    {
        Disconnect();
    }

    public override void FixedUpdate(float fixedDelta)
    {
    }
}
