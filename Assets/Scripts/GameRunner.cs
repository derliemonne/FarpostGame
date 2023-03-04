using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public enum ConnectionStatus {
    NotInSession,
    ConnectingToSession,
    InSession,
}


public class GameRunner : MonoBehaviour, INetworkRunnerCallbacks
{
    public static GameRunner Instance { get; private set; }

    /// <summary>
    /// Previous ConnectionStatus and new ConnectionStatus.
    /// </summary>
    public event Action<ConnectionStatus, ConnectionStatus> ConnectionStatusChanged;
    public event Action GameStateHandlerSpawnedLocally;
    
    public ConnectionStatus ConnectionStatus
    {
        get => _connectionStatus;
        private set
        {
            ConnectionStatusChanged?.Invoke(_connectionStatus, value);
            _connectionStatus = value;
        }
    }
    
    public PlayerSound PlayerSound => _playerSound;
    [Header("Prefabs")]
    [SerializeField] private NetworkManager _networkManagerPrefab;
    [SerializeField] private GameStateHandler _gameStateHandlerPrefab;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private LevelManager _levelManagerPrefab;
    [SerializeField] private CrateSpawner _crateSpawnerPrefab;
    [Space]
    [SerializeField] private PlayerSound _playerSound;
    [SerializeField] private Gui _gui;

    
    private NetworkManager _networkManager;
    private ConnectionStatus _connectionStatus = ConnectionStatus.NotInSession;

    public void NotifyGameStateHandlerSpawnedLocally()
    {
        GameStateHandlerSpawnedLocally?.Invoke();
        
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        Debug.Assert(gameStateHandler != null);
        if (gameStateHandler == null) return;
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        

        _gui.StartHost += OnGuiStartHost;
        _gui.ConnectToRoomWithId += OnGuiConnectToRoom;
        _gui.LeaveSession += OnGuiLeaveSession;
    }
    
    private async void ConnectToRoom(GameMode gameMode, string sessionName)
    {
        ConnectionStatus = ConnectionStatus.ConnectingToSession;
        
        NetworkManager networkManager = Instantiate(_networkManagerPrefab);
        _networkManager = networkManager;
        _networkManager.NetworkRunner.ProvideInput = true;
        _networkManager.NetworkRunner.AddCallbacks(this);

        await _networkManager.NetworkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = sessionName,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = GameStateHandler.MaxPlayersInSession
        });
        
        if (gameMode == GameMode.Host)
        {
            GameStateHandler gameStateHandler = _networkManager.NetworkRunner.Spawn(_gameStateHandlerPrefab);
            if (gameStateHandler == null)
            {
                Debug.LogError($"{nameof(gameStateHandler)} is null.");
                return;
            }
        }
        
        ConnectionStatus = ConnectionStatus.InSession;
    }
    
    private void OnGuiStartHost()
    {
        string sessionName = Random.Range(0, 100).ToString();
        ConnectToRoom(GameMode.Host, sessionName);
    }
    
    private void OnGuiConnectToRoom(int roomId)
    {
        ConnectToRoom(GameMode.Client, sessionName: roomId.ToString());
    }
    
    private void OnGuiLeaveSession()
    {
        // TODO
    }

    private void OnDestroy()
    {
        if (_gui == null)
        {
            return;
        }
        _gui.ConnectToRoomWithId -= OnGuiConnectToRoom;
        _gui.StartHost -= OnGuiStartHost;
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef joinedPlayerRef)
    {
        Debug.Log("> OnPlayerJoined");
        
        if (runner.IsServer)
        {
            bool roomIsOverfull = _networkManager.NetworkRunner.ActivePlayers.Count() >
                              GameStateHandler.MaxPlayersInSession;
            bool canConnect = !roomIsOverfull;
            if (GameStateHandler.Instance != null && GameStateHandler.Instance.RaceStarted)
            {
                canConnect = false;
            }
            if (!canConnect)
            {
                runner.Disconnect(joinedPlayerRef);
            }
            
            Player player = runner.Spawn(_playerPrefab, inputAuthority: joinedPlayerRef);
            if (player == null)
            {
                Debug.LogError($"{nameof(_playerPrefab)} is invalid");
                return;
            }
            player.PlayerRef = joinedPlayerRef;
            runner.SetPlayerObject(joinedPlayerRef, player.Object);
        }
        
        if (NetworkManager.Instance.IsPlayerHost(joinedPlayerRef.PlayerId))
        { 
            LevelManager levelManager = runner.Spawn(_levelManagerPrefab);

            CrateSpawner crateSpawner = Instantiate(_crateSpawnerPrefab);
            if (crateSpawner == null)
            {
                Debug.LogError($"{nameof(_crateSpawnerPrefab)} is invalid.");
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
    {
        Debug.Log("> OnPlayerLeft");
        if (NetworkManager.Instance.TryGetPlayerObject(playerRef, out Player player))
        {
            if (player.Character != null && player.Character.Object != null)
            {
                runner.Despawn(player.Character.Object);
            }

            if (player.Object != null)
            {
                runner.Despawn(player.Object);
            }
        }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        bool jumped = false;
        bool pushedPlatform = false;
        int moveDirection = 0;
        
        Gui gui = Gui.Instance;
        if (gui != null)
        {
            jumped = gui.UpInputButton.IsPressed;
            if (gui.PushPlatformButton == null)
            {
                Debug.LogError("PushPlatformButton is null");
            }
            else
            {
                pushedPlatform = gui.PushPlatformButton.IsPressed;
            }
            if (gui.RightInputButton.IsPressed) moveDirection = 1;
            if (gui.LeftInputButton.IsPressed) moveDirection = -1;
        }
        
        input.Set(new NetworkInputData
        {
            Direction = moveDirection,
            Jumped = jumped, 
            PushedPlatform = pushedPlatform,
        });
    }
    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"> OnNetworkRunnerShutdown: {nameof(shutdownReason)}: {shutdownReason}");
        ConnectionStatus = ConnectionStatus.NotInSession;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log($"> OnConnectedToServer: playerId {runner.LocalPlayer.PlayerId}.");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("> OnDisconnectedFromServer");

        Gui gui = Gui.Instance;
        if (gui == null)
        {
            Debug.LogError($"{nameof(gui)} is null");
        }
        else
        {
            gui.ShowMessageBox(NetworkManager.NetworkMessages.GetValueOrDefault(
                NetworkMessage.ServerKickedRaceStarted, "Потеряно соединение с сервером."));
        }

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
}

// public partial class :  INetworkRunnerCallbacks
// {
//     
// }
