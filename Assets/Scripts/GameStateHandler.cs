using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public enum ConnectionStatus {
    NotInSession,
    ConnectingToSession,
    InSession,
}

public class GameStateHandler : NetworkBehaviour, INetworkRunnerCallbacks
{
    public const int MaxPlayersInSession = 4;
    private const float GuiScaleForPhones = 3.5f;
    
    public static GameStateHandler Instance { get; private set; }
    
    [Networked] public NetworkBool RaceStarted { get; private set; }
    public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.NotInSession;
    
    [SerializeField] private NetworkManager _networkManagerPrefab;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private NetworkPrefabRef _characterPrefab;
    [SerializeField] private LevelBuilder _levelBuilderPrefab;
    [SerializeField] private Vector2 _startPlayerPos = new(0, 0);
    [SerializeField] private CameraScript _cs;
    [SerializeField] private PlayersList _playersList;
    [SerializeField] private DeathZoneScript _deathZoneScript;

    private bool _readyToStartRace;
    private NetworkManager _networkManager;
    private string _inputSessionName;


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef joinedPlayerRef)
    {
        Debug.Log("> OnPlayerJoined");
        if (runner.IsServer)
        {
            Player player = runner.Spawn(_playerPrefab, inputAuthority: joinedPlayerRef).GetComponent<Player>();
            player.PlayerRef = joinedPlayerRef;
            runner.SetPlayerObject(joinedPlayerRef, player.Object);

            if (_networkManager.IsPlayerHost(joinedPlayerRef.PlayerId))
            { 
                LevelBuilder levelBuilder = Instantiate(_levelBuilderPrefab);
                levelBuilder.SpawnChunkOnTop();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
    {
        Debug.Log("> OnPlayerLeft");
        if (_networkManager.TryGetPlayerObject(playerRef, out Player player))
        {
            runner.Despawn(player.Object);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(new NetworkInputData
        {
            Direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxis("Vertical"))
        });
    }
    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("> OnNetworkRunnerShutdown");
        ConnectionStatus = ConnectionStatus.NotInSession;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("> OnConnectedToServer");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("> OnDisconnectedFromServer");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("> OnConnectRequest");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("> OnConnectFailed");
    }
    
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("> OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("> OnSceneLoadStart");
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private async void ConnectToRoom(GameMode mode, string sessionName="TestRoom")
    {
        ConnectionStatus = ConnectionStatus.ConnectingToSession;
        
        // Create the Fusion runner and let it know that we will be providing user input
        NetworkManager networkManager = Instantiate(_networkManagerPrefab);
        _networkManager = networkManager;
        _networkManager.NetworkRunner.AddCallbacks(this);
        _networkManager.NetworkRunner.ProvideInput = true;

        // Start or join (depends on GameMode) a session
        await _networkManager.NetworkRunner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = MaxPlayersInSession
        });
        
        ConnectionStatus = ConnectionStatus.InSession;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_OnPlayerChangedReadyToStartRace(bool readyToStart, RpcInfo rpcInfo=default)
    {
        PlayerRef playerRef = rpcInfo.Source;
        Debug.Log($"> RPC on {_networkManager.NetworkRunner.LocalPlayer.PlayerId} from {playerRef.PlayerId}.");
        if (_networkManager.TryGetPlayerObject(playerRef, out Player player))
        {
            player.IsReadyToStartRace = readyToStart;
        }

        bool allPlayersAreReady = _networkManager.NetworkRunner.ActivePlayers
            .Select(playerRef => _networkManager.GetPlayerObject(playerRef))
            .All(player => player != null && player.IsReadyToStartRace);
        if (allPlayersAreReady)
        {
            RaceStarted = true;
            StartRace();
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_BindCamera([RpcTarget] PlayerRef player, NetworkObject playerGameObject)
    {
        _cs.BindPlayer(playerGameObject.transform, playerGameObject.GetComponent<Rigidbody2D>());
    }

    /// <summary>
    /// Executes on host.
    /// </summary>
    private void StartRace()
    {
        bool notYet = true;
        if (!_networkManager.NetworkRunner.IsServer)
        {
            Debug.LogWarning("Host method executes on client.");
            return;
        }
        foreach (Player player in _networkManager.GetActivePlayers())
        {
            player.transform.position = _startPlayerPos;
            _networkManager.NetworkRunner.Spawn(_characterPrefab, _startPlayerPos, inputAuthority: player.PlayerRef);
            // _characterSpawner.SpawnCharacter(Vector3.zero, Quaternion.identity, player.transform);
        }
        _deathZoneScript.SetActive(true);
    }

    // This GUI should be replaced to normal GUI with assets in future.
    private void OnGUI()
    {
        // Scale GUI that it is not too small on phones.
        GUI.matrix = Matrix4x4.TRS(
            Vector3.zero, Quaternion.identity,new Vector3(GuiScaleForPhones, GuiScaleForPhones, 1));
        GUILayout.Label($"ConnectionStatus: {ConnectionStatus}");
        if (Object != null)
        {
            GUILayout.Label($"NetworkObject HasStateAuthority: {Object.HasStateAuthority}");
            GUILayout.Label($"RaceStarted: {RaceStarted}");
        }
        if (_networkManager != null)
        {
            GUILayout.Label($"LocalPlayer: {_networkManager.NetworkRunner.LocalPlayer.PlayerId}");
        }
        if (ConnectionStatus == ConnectionStatus.NotInSession)
        {
            if (GUILayout.Button("Host"))
            {
                string sessionName = Random.Range(0, 100).ToString();
                ConnectToRoom(GameMode.Host, sessionName);
            }
            GUILayout.Label("Input room name:");
            _inputSessionName = GUILayout.TextField(_inputSessionName);
            if (GUILayout.Button("Connect"))
            {
                ConnectToRoom(GameMode.Client, _inputSessionName);
            }
        }
        else if (ConnectionStatus == ConnectionStatus.ConnectingToSession)
        {
            GUILayout.Label("Connecting to room...");
        }
        else if (ConnectionStatus == ConnectionStatus.InSession && _networkManager != null && Object != null)
        {
            GUILayout.Label($"Room name: {_networkManager.NetworkRunner.SessionInfo.Name}");
            
            int playersConnectedToRoom = _networkManager.NetworkRunner.SessionInfo.PlayerCount;
            GUILayout.Label($"{playersConnectedToRoom}/{MaxPlayersInSession} players connected.");

            if (!RaceStarted)
            {
                if (GUILayout.Button(_readyToStartRace ? "Ready" : "Not ready"))
                {
                    _readyToStartRace = !_readyToStartRace;
                    RPC_OnPlayerChangedReadyToStartRace(_readyToStartRace);
                }
            
                // Draw other connected players and their ready status.
                foreach (PlayerRef playerRef in _networkManager.NetworkRunner.ActivePlayers)
                {
                    if (_networkManager.TryGetPlayerObject(playerRef, out Player player))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(player.PlayerRef.PlayerId.ToString());
                        GUILayout.Label(player.IsReadyToStartRace ? "Ready" : "Not Ready");
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
    
    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
