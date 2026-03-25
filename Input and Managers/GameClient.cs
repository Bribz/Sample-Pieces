using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using LiteNetLib;
using LiteNetLib.Utils;

using THNetworkLibrary;
using THNetworkLibrary.Entities;
using THNetworkLibrary.Math;
using THNetworkLibrary.Packets;
using THNetworkLibrary.Packets.Gameplay;
using THNetworkLibrary.Tools;

namespace THGameClient
{
    public class GameClient
    {
        //NetManager Components
        private NetManager NetManager;
        private EventBasedNetListener _listener;
        private NetPacketProcessor _packetProcessor;
        private NetDataWriter _cachedWriter;

        //Client Logic
        private LogicTimer _logicTimer;
        private PacketDispatcher _packetDispatcher;
        private NetworkObjectManager NetworkObjectManager;
        private NetPeer ServerPeer;
        private IPEndPoint ServerEndPoint;

        private IInputPoller _inputPoller;
        public ClientInputHandler InputHandler;

        private ServerState _cachedServerState = new ServerState();
        private ushort _lastServerTick;

        #region Events
        public delegate void DisconnectionEvent();
        public event DisconnectionEvent OnDisconnectedFromServer;

        public delegate void ConnectionEvent();
        public event ConnectionEvent OnConnectedToServer;
        #endregion

        private uint PlayerEntity_NetID;

        //Utility
        private DebugNetworkSettings DebugNetworkSettings;
        public NetStatistics NetworkStats;
        public long BytesReceivedPerSecond;
        public long BytesSentPerSecond;
        public long PacketLossPercent;
        public int Ping = 0;
        public ServerConnectionState ConnectionState = ServerConnectionState.Disconnected;

        /// <summary>
        /// Create a GameClient object.
        /// </summary>
        /// <param name="poller">Class that manages Input to pass to the Server</param>
        /// <param name="objManager">Class that manages instantiation of Network objects</param>
        public GameClient(IInputPoller poller, NetworkObjectManager objManager)
        {
            _inputPoller = poller;
            _logicTimer = new LogicTimer(LogicUpdate);
            _cachedWriter = new NetDataWriter();

            _listener = new EventBasedNetListener();
            RegisterListenerEvents();

            _packetProcessor = new NetPacketProcessor();
            _packetDispatcher = new PacketDispatcher(_cachedWriter, _packetProcessor);

            if (objManager == null)
            {
                NetworkObjectManager = new NetworkObjectManager();
            }
            else
            {
                NetworkObjectManager = objManager;
            }

            InputHandler = new ClientInputHandler(_inputPoller);

            //Subscribe packet callbacks
            _packetDispatcher.SubscribeReusable<ConnectionAcceptPacket>(OnConnectionRequestAccepted);
            _packetDispatcher.SubscribeReusable<ConnectionRejectPacket>(OnConnectionRequestRejected);
            _packetDispatcher.SubscribeReusable<ClientConnected>(OnNewClientData);
            _packetDispatcher.SubscribeReusable<ClientDisconnected>(OnClientDisconnected);
            _packetDispatcher.SubscribeReusable<SkillUsedPacket>(NetworkObjectManager.ApplySkillUsed);
            _packetDispatcher.SubscribeReusable<ClearNetworkObjectPacket>((ClearNetworkObjectPacket packet) => { Log.Debug("ClearNetworkObjectPacket Entered"); objManager.ClearNetworkObject(packet.NetworkID); });
            _packetDispatcher.SubscribeReusable<SpawnNetworkObjectPacket>((SpawnNetworkObjectPacket packet) => { Log.Debug("SpawnNetworkObjectPacket Entered"); objManager.SpawnNetworkObject(packet.DataID, packet.NetworkID, packet.InitialPosition, packet.InitialRotation); });
            _packetDispatcher.SubscribeReusable<SpawnNetworkEntityPacket>((SpawnNetworkEntityPacket packet) => { Log.Debug("SpawnNetworkEntityPacket Entered"); objManager.SpawnNetworkEntity(packet.DataID, packet.NetworkID, packet.InitialPosition, packet.InitialRotation); });
            _packetDispatcher.RegisterSerializedPacketCallback(PacketType.SpawnInitialization, OnSpawnListReceived);
            _packetDispatcher.RegisterSerializedPacketCallback(PacketType.ServerState, OnServerStateReceive);
            
        }

        #region Connect/Disconnect
        /// <summary>
        /// Connect to a server.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="settings"></param>
        public void Connect(IPEndPoint endPoint, string username, string password, DebugNetworkSettings settings = null)
        {
            if (ServerPeer != null) //Don't run this function if the server is already running
            {
                Log.Warning("Attempted to connect to client while already connected!");
                return;
            }
            ServerEndPoint = endPoint;

            ConnectionState = ServerConnectionState.Connecting;
            
            if(NetManager == null)
            {
                NetManager = new NetManager(_listener)
                {   
                    EnableStatistics = true, 
                    AutoRecycle = true
                };
            }
            ApplyDebugNetworkConditions(settings);
            NetManager.Start();

            NetworkStats = NetManager.Statistics;

            Log.SystemMessage("Connecting to Game Server...");

            //Write a connection request packet to include with the LiteNetLib ConnectionRequest
            var requestPacket = new ConnectionRequestPacket()
            {
                Username = username, //Environment.MachineName.ToString(),
                Password = password,
                VersionKey = NetworkGeneral.VersionID
            };

            //Write packet to NetDataWriter
            _cachedWriter.Reset();
            _cachedWriter.Put((byte)0);
            _cachedWriter.Put(requestPacket.Username);
            _cachedWriter.Put(requestPacket.Password);
            _cachedWriter.Put(requestPacket.VersionKey);

            //Send the request
            NetManager.Connect(endPoint, _cachedWriter);
        }

        /// <summary>
        /// Disconnect from an established server connection. 
        /// </summary>
        public void Disconnect()
        {
            _logicTimer.Stop();
            if (NetManager != null)
                NetManager.Stop();
        }
        #endregion

        /// <summary>
        /// Run the client. Normally this would be handled within this class, but this part is separated to allow multithreading within Unity.
        /// This would be in the case where alternative means are necessary due to Unity MainThread nonsense.
        /// </summary>
        public void RunClient()
        {
            NetManager.PollEvents();
            _logicTimer.Update();
        }

        int i = 0; //Temporary
        /// <summary>
        /// Logic Update called by the LogicTimer handled during connection with the server. This won't be called if the LogicTimer isn't active.
        /// </summary>
        public void LogicUpdate()
        {
            if (ConnectionState != ServerConnectionState.Connected)
                return;

            if (i%60 == 0)
            {
                //Log.Message("Logic Timer Update");
                UpdateNetworkStatistics();
            }
            i++;

            NetworkObjectManager.Tick();

            //Send Inputs to server
            var nextCommand = InputHandler.GetNextCommand();
            nextCommand.CommandID = (ushort)((nextCommand.CommandID + 1) % NetworkGeneral.MaxGameSequence);
            nextCommand.ACK = _lastServerTick; //_cachedServerState.Tick;
            SendSerializable(PacketType.PlayerInput, nextCommand);
        }

        /// <summary>
        /// Send a packet to the server. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <param name="method"></param>
        public void SendPacket<T>(T packet, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            ServerPeer.Send(_packetDispatcher.WritePacket(packet), method);
        }

        /// <summary>
        /// Send a serialized packet to the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        /// <param name="method"></param>
        public void SendSerializable<T>(PacketType type, T packet, DeliveryMethod method = DeliveryMethod.Unreliable) where T : struct, INetSerializable
        {
            ServerPeer.Send(_packetDispatcher.WriteSerializable(type, packet), method);
        }

        #region NetworkEvents

        /// <summary>
        /// Registers a series of events called by the EventBasedNetListener object maintained by the Client. Should be called when the NetListener is created.
        /// </summary>
        private void RegisterListenerEvents()
        {
            //Utility
            _listener.NetworkErrorEvent += OnNetworkError;
            _listener.NetworkLatencyUpdateEvent += OnLatencyUpdate;

            //Peer Connection
            _listener.ConnectionRequestEvent += OnConnectionRequest;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;

            //Data Receive
            _listener.NetworkReceiveEvent += OnDataReceive;
            _listener.NetworkReceiveUnconnectedEvent += OnDataReceive_Unconnected;
        }

        #region Utility

        /// <summary>
        /// Callback when the Client receives a currently latency update from the Server.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="latency"></param>
        private void OnLatencyUpdate(NetPeer peer, int latency)
        {
            Ping = latency;
        }

        /// <summary>
        /// Callback when an error is encountered during Network Transmission.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="socketError"></param>
        private void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
        {
            Log.Error($"Network Error Detected: {endPoint.Address} / {socketError.ToString()}");
        }

        #endregion

        #region PeerConnections

        /// <summary>
        /// Callback when a ConnectionRequest is received by the Client. This is unintended behavior on the Client's side.
        /// </summary>
        /// <param name="request"></param>
        private void OnConnectionRequest(ConnectionRequest request)
        {
            Log.Warning("Client received a connection request. Rejecting...");
            request.Reject();
        }

        /// <summary>
        /// Callback when a ConnectionRequest is accepted by the Server.
        /// DEPRECATED. Might be needed later, but currently the Client doesn't set up the PacketDispatcher until a connection is established in the first place.
        /// </summary>
        /// <param name="packet"></param>
        private void OnConnectionRequestAccepted(ConnectionAcceptPacket packet)
        {
            Log.SystemMessage($"Connection request to Server accepted! :\n    SessionToken - {packet.SessionToken} /\n    NetID - {packet.NetworkID} /\n    UserID - {packet.PolledUserID} /\n    Username - {packet.PolledUsername} /\n    CharID - {packet.PolledCharacterID} /\n    PlayerEntityNetID - {packet.AttachedEntityNetworkID}");
            _lastServerTick = packet.ServerTick;
            PlayerEntity_NetID = packet.AttachedEntityNetworkID;
            _packetDispatcher.SetSessionToken(Convert.FromBase64String(packet.SessionToken));

            _logicTimer.Start();

            //We set the state to connected only after accepted by the server to avoid confusion within engine.
            //We set the state to disconnected when dropped by LiteNetLib
            ConnectionState = ServerConnectionState.Connected;
            OnConnectedToServer?.Invoke();
        }

        /// <summary>
        /// Callback called when a ConnectionRequest is rejected by the Server. 
        /// DEPRECATED. Might be needed later, but currently the Client doesn't set up the PacketDispatcher until a connection is established in the first place. 
        /// </summary>
        /// <param name="packet"></param>
        private void OnConnectionRequestRejected(ConnectionRejectPacket packet)
        {
            Log.SystemMessage($"Connection request to Server rejected! : {packet.Reason.ToString()}");
        }

        /// <summary>
        /// Callback called by the Server when another client disconnects. Reflects data to Client.
        /// </summary>
        /// <param name="packet"></param>
        private void OnClientDisconnected(ClientDisconnected packet)
        {
            //TODO: Implement local remote client management.
        }

        /// <summary>
        /// Callback called when a connection is established with the Server.
        /// </summary>
        /// <param name="peer"></param>
        private void OnPeerConnected(NetPeer peer)
        {
            Log.Message($"Connected to server - {peer.EndPoint.Address}", LogType.Debug);

            if(ServerPeer == null)
            {
                ServerPeer = peer;
            }
        }

        /// <summary>
        /// Callback called when the Client disconnects from the Server
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="disconnectInfo"></param>
        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Message($"Disconnected from server - {peer.EndPoint.Address} - {disconnectInfo.Reason}", LogType.Debug);

            ServerPeer = null;
            _logicTimer.Stop();

            ConnectionState = ServerConnectionState.Disconnected;
            OnDisconnectedFromServer?.Invoke();
        }

        #endregion

        #region Data Receive

        /// <summary>
        /// Callback when the Server has a new client joined. 
        /// </summary>
        /// <param name="packet"></param>
        private void OnNewClientData(ClientConnected packet)
        {
            Log.Message($"New Client Connected to Server : {packet.Username} - NetID : {packet.NetworkID} / UserID : {packet.UserID} / CharID : {packet.CharacterID}");
        }

        /// <summary>
        /// Callback when the Server sends the initial state of the server.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="peer"></param>
        private void OnSpawnListReceived(NetDataReader reader, NetPeer peer)
        {
            Log.Message("Received NetworkObject Initialization packet from Server.");

            var initializationList = new ObjectInitializationPacket();
            initializationList.Deserialize(reader);

            foreach(var obj in initializationList.NetworkObjectList)
            {
                NetworkObjectManager.SpawnNetworkObject(obj.DataID, obj.NetworkID, obj.Position, obj.Rotation);
            }
            foreach(var obj in initializationList.NetworkEntityList)
            {
                NetworkObjectManager.SpawnNetworkEntity(obj.DataID, obj.NetworkID, obj.Position, obj.Rotation);
            }
        }

        /// <summary>
        /// Callback when the Client receives a state update from the Server. 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="peer"></param>
        private void OnServerStateReceive(NetDataReader reader, NetPeer peer)
        {
            var serverTick = reader.GetUShort();
            var lastProcessedCommand = reader.GetUShort();

            //_cachedServerState.Deserialize(reader);

            if(NetworkGeneral.SeqDiff(serverTick, _lastServerTick) <= 0)
            {
                return;
            }
            _lastServerTick = serverTick; //_cachedServerState.Tick;
            //Apply the server state to the Network Object manager
            NetworkObjectManager.ApplyServerState(reader);
        }

        /// <summary>
        /// Callback when a packet has been received by the Client from the Server. This is handled automatically by the NetManager's PollEvents function.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="reader"></param>
        /// <param name="deliveryMethod"></param>
        private void OnDataReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            _packetDispatcher.DispatchPacket(peer, reader, deliveryMethod);
        }

        /// <summary>
        /// Callback when the client receives an unconnected message. At the moment, this is unintended, but might have use later.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="reader"></param>
        /// <param name="messageType"></param>
        private void OnDataReceive_Unconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            //Ignore Unconnected Packets
        }

        #endregion

        #endregion

        #region Utility

        /// <summary>
        /// Apply Debug network conditions to the NetManager for testing purposes.
        /// </summary>
        /// <param name="settings"></param>
        private void ApplyDebugNetworkConditions(DebugNetworkSettings settings)
        {
            if (settings != null)
            {
                NetManager.SimulateLatency = settings.SimulatedLatency != 0;
                if (NetManager.SimulateLatency)
                {
                    NetManager.SimulationMinLatency = (settings.SimulatedLatency - (settings.SimulatedLatencyRange / 2)).Clamp(
                        0,
                        DebugNetworkSettings.MAXIMUM_DEBUG_LATENCY);
                    NetManager.SimulationMinLatency = (settings.SimulatedLatency + (settings.SimulatedLatencyRange / 2)).Clamp(
                        0,
                        DebugNetworkSettings.MAXIMUM_DEBUG_LATENCY);
                }

                NetManager.SimulatePacketLoss = settings.SimulatedPacketLossChance != 0;
                if (NetManager.SimulatePacketLoss)
                {
                    NetManager.SimulationPacketLossChance = settings.SimulatedPacketLossChance.Clamp(1, 100);
                }
            }
        }

        /// <summary>
        /// Update the GameClient tracked NetworkStatistics
        /// </summary>
        private void UpdateNetworkStatistics()
        {
            BytesReceivedPerSecond = NetworkStats.BytesReceived;
            BytesSentPerSecond = NetworkStats.BytesSent;
            PacketLossPercent = NetworkStats.PacketLossPercent;
            NetworkStats.Reset();
        }
        #endregion
    }
}
