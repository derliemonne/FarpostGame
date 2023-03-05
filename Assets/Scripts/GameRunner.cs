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


public partial class GameRunner : MonoBehaviour
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
    
    [Header("Prefabs")]
    [SerializeField] private NetworkManager _networkManagerPrefab;
    [SerializeField] private GameStateHandler _gameStateHandlerPrefab;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private LevelManager _levelManagerPrefab;
    [SerializeField] private CrateSpawner _crateSpawnerPrefab;
    [SerializeField] private PlayerSound _playerSoundPrefab;
    [Space]
    [SerializeField] private Gui _gui;
    [SerializeField] private CameraScript _cameraScript;
    
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
        _gui.LeavedSession += OnGuiLeavedSession;
    }
    
    private async void ConnectToRoom(GameMode gameMode, string sessionName)
    {
        ConnectionStatus = ConnectionStatus.ConnectingToSession;
        
        NetworkManager networkManager = Instantiate(_networkManagerPrefab);
        _networkManager = networkManager;
        _networkManager.NetworkRunner.ProvideInput = true;
        _networkManager.NetworkRunner.AddCallbacks(this);

        StartGameResult result = await _networkManager.NetworkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = sessionName,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = GameStateHandler.MaxPlayersInSession
        });
        Debug.Log("StartGameResult: " +
                  $"{nameof(result.Ok)}: {result.Ok}, " +
                  $"{nameof(result.ErrorMessage)}: {result.ErrorMessage}, " +
                  $"{nameof(result.ShutdownReason)}: {result.ShutdownReason}.");
        if (!result.Ok)
        {
            ConnectionStatus = ConnectionStatus.NotInSession;
            return;
        }
        
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
        if (_networkManager == null)
        {
            ConnectToRoom(GameMode.Host, sessionName);
        }
        else
        {
            GuiMessageLocalUser("Network error. Try again.");
            Debug.LogError($"NetworkError. Can not start host. {nameof(_networkManager)} already exists.");
        }
    }
    
    private void OnGuiConnectToRoom(int roomId)
    {
        if (_networkManager == null)
        {
            ConnectToRoom(GameMode.Client, sessionName: roomId.ToString());
        }
        else
        {
            GuiMessageLocalUser("Network error. Try again.");
            Debug.LogError($"NetworkError. Can not connect to room. {nameof(_networkManager)} already exists.");
        }
    }
    
    private void OnGuiLeavedSession()
    {
        _networkManager.NetworkRunner.Shutdown();
    }

    private void GuiMessageLocalUser(string message)
    {
        if (_gui == null)
        {
            Debug.LogWarning($"{nameof(_gui)} is null.");
        }
        else
        {
            _gui.ShowMessageBox(message);
        }
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
}

public partial class GameRunner :  INetworkRunnerCallbacks
{
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

            CrateSpawner crateSpawner = runner.Spawn(_crateSpawnerPrefab);
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
        Destroy(_networkManager.gameObject);
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
