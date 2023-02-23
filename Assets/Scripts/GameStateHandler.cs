using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public class GameStateHandler : NetworkBehaviour, INetworkRunnerCallbacks
{
    public const int MaxPlayersInSession = 4;
    
    private const float GuiScaleForPhones = 3.5f;

    public static GameStateHandler Instance { get; private set; }

    /// <summary>
    /// Previous ConnectionStatus and new ConnectionStatus.
    /// </summary>
    public event Action<ConnectionStatus, ConnectionStatus> ConnectionStatusChanged;
    public event Action<bool> RaceStartedChanged;
    public event Action<List<Record>> RaceFinished;
    
    [Networked(OnChanged = nameof(OnRaceStartedChanged))] 
    public NetworkBool RaceStarted { get; private set; }
    
    [Networked(OnChanged = nameof(OnRaceFinishedChanged))]
    public NetworkBool IsRaceFinished { get; private set; }
    
    public ConnectionStatus ConnectionStatus
    {
        get => _connectionStatus;
        private set
        {
            ConnectionStatusChanged?.Invoke(_connectionStatus, value);
            _connectionStatus = value;
        }
    }

    public ReadOnlyCollection<Character> CharacterPrefabs => _characterPrefabs.Values.ToList().AsReadOnly(); 
    
    [SerializeField] private NetworkManager _networkManagerPrefab;
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private SerializableDictionary<CharacterType, Character> _characterPrefabs;
    [SerializeField] private LevelManager _levelManagerPrefab;
    [SerializeField] private Darkness _darknessPrefab;
    [SerializeField] private CrateSpawner _crateSpawnerPrefab;
    [SerializeField] private Vector2 _startCharacterPosition = new(0, 0);
    [SerializeField] private CameraScript _cs;
    [SerializeField] private Gui _gui;
    [SerializeField] private bool _drawDebugGui;

    private bool _readyToStartRace;
    private NetworkManager _networkManager;
    private string _inputSessionName;
    private ConnectionStatus _connectionStatus = ConnectionStatus.NotInSession;

    private bool jumpPressed = false;
    private bool pushPlatformPressed = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        if (_gui == null)
        {
            Debug.LogError("Gui is null");
            return;
        }
        _gui.ConnectToRoomWithId += OnGuiConnectToRoom;
        _gui.StartHost += OnGuiStartHost;
        _gui.CharacterChosenAction += OnGuiChosenCharacter;
        _gui.ReadyToStartRaceChanged += OnGuiChangedReadyToStartRace;
    }
    
    public static void OnRaceStartedChanged(Changed<GameStateHandler> changed)
    {
        changed.Behaviour.RaceStartedChanged?.Invoke(changed.Behaviour.RaceStarted);
    }

    public static void OnRaceFinishedChanged(Changed<GameStateHandler> changed)
    {
        List<Record> records = new();
        foreach (Player player in NetworkManager.Instance.GetActivePlayers())
        {
            if (player.Character == null)
            {
                Debug.LogError("player.Character is null");
            }
            records.Add(new Record
            {
                Name = Character.CharacterNames.TryGetValue(player.ChosenCharacter, out string name) 
                    ? name 
                    : "анон",
                MaxAltitude = player.Character == null ? 0 : (int)player.Character.AltitudeRecord
            });
        }
        records.Sort((record1, record2) => record2.MaxAltitude.CompareTo(record1.MaxAltitude));
        for (int i = 0; i < records.Count; i++)
        {
            Record record = records[i];
            Debug.Log($"GameStateHandler: {record.MaxAltitude} {i + 1}");
            record.Place = i + 1;
            records[i] = record;
        }
        changed.Behaviour.RaceFinished?.Invoke(records);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef joinedPlayerRef)
    {
        Debug.Log("> OnPlayerJoined");
        if (runner.IsServer)
        {
            Player player = runner.Spawn(_playerPrefab, inputAuthority: joinedPlayerRef);
            if (player == null)
            {
                Debug.LogError($"{nameof(_playerPrefab)} is invalid");
                return;
            }
            player.PlayerRef = joinedPlayerRef;
            runner.SetPlayerObject(joinedPlayerRef, player.Object);
        }
        
        if (_networkManager.IsPlayerHost(joinedPlayerRef.PlayerId))
        { 
            LevelManager levelManager = Runner.Spawn(_levelManagerPrefab);

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
        if (_networkManager.TryGetPlayerObject(playerRef, out Player player))
        {
            runner.Despawn(player.Character.Object);
            runner.Despawn(player.Object);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(new NetworkInputData
        {
            Direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxis("Vertical")),
            Jumped = jumpPressed,
            PushedPlatform = pushPlatformPressed
        });
        jumpPressed = false;
        pushPlatformPressed = false;
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

    private void Update()
    {
        jumpPressed |= Input.GetButtonDown("Jump");
        pushPlatformPressed |= Input.GetButtonDown("PushPlatform");
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
    private void Rpc_OnPlayerChangedReadyToStartRace(bool readyToStart, RpcInfo rpcInfo=default)
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnPlayerChooseCharacter(CharacterType characterType, RpcInfo rpcInfo=default)
    {
        if (NetworkManager.Instance.TryGetPlayerObject(rpcInfo.Source, out Player player))
        {
            player.ChosenCharacter = characterType;
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_BindCamera([RpcTarget] PlayerRef player, Character character)
    {
        _cs.BindPlayer(character.transform, character.GetComponent<Rigidbody2D>(), character);
    }

    /// <summary>
    /// Executes on host.
    /// </summary>
    private void StartRace()
    {
        if (!_networkManager.NetworkRunner.IsServer)
        {
            Debug.LogError("Host method executes on client.");
            return;
        }
        foreach (Player player in _networkManager.GetActivePlayers())
        {
            player.transform.position = _startCharacterPosition;
            if (!_characterPrefabs.TryGetValue(player.ChosenCharacter, out Character chosenPrefab))
            {
                Debug.LogError("Race started without player chose character.");
                chosenPrefab = _characterPrefabs.Values.ToList()[0];
            }
            if (chosenPrefab == null)
            {
                Debug.LogError("Character prefab is invalid.");
                return;
            }
            Character character = _networkManager.NetworkRunner
                .Spawn(chosenPrefab, _startCharacterPosition, inputAuthority: player.PlayerRef);
            character.SetPlayerId(player.PlayerRef.PlayerId);
            if (character == null)
            {
                Debug.LogError($"Character prefab is invalid.");
            }
            player.Character = character;
            character.Died += OnCharacterDied;
            
            Rpc_BindCamera(player.PlayerRef, character);
        }

        // Darkness darkness = _networkManager.NetworkRunner.Spawn(_darknessPrefab, new Vector3(-100, -100, -100), Quaternion.identity);
        Darkness darkness = _networkManager.NetworkRunner.Spawn(_darknessPrefab);
        if (darkness == null)
        {
            Debug.LogError($"{nameof(_darknessPrefab)} is invalid.");
        }
        darkness.SetActive(true);
    }

    /// <summary>
    /// Server-only.
    /// </summary>
    private void OnCharacterDied(Character characterDied, float altitudeRecord)
    {
        if (!NetworkManager.Instance.NetworkRunner.IsServer) return;
        if (NetworkManager.Instance.GetActivePlayers()
            .Select(player => player.Character)
            .All(character => character != null && character.IsDead))
        {
            IsRaceFinished = true;
        }
    }

    private void OnGuiConnectToRoom(int roomId)
    {
        ConnectToRoom(GameMode.Client, _inputSessionName);
    }

    private void OnGuiStartHost()
    {
        string sessionName = Random.Range(0, 100).ToString();
        ConnectToRoom(GameMode.Host, sessionName);
    }

    private void OnGuiChosenCharacter(CharacterType characterType)
    {
        if (Object != null)
        {
            RPC_OnPlayerChooseCharacter(characterType);
        }
    }

    private void OnGuiChangedReadyToStartRace(bool readyToStartRace)
    {
        _readyToStartRace = readyToStartRace;
        Rpc_OnPlayerChangedReadyToStartRace(readyToStartRace);
    }

    // This GUI should be replaced to normal GUI with assets in future.
    private void OnGUI()
    {
        if (!_drawDebugGui) return;
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
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_readyToStartRace ? "Ready" : "Not ready"))
                {
                    _readyToStartRace = !_readyToStartRace;
                    Rpc_OnPlayerChangedReadyToStartRace(_readyToStartRace);
                }

                foreach (CharacterType characterType in _characterPrefabs.Keys)
                {
                    Character prefab = _characterPrefabs[characterType];
                    if (GUILayout.Button(prefab.CharacterName))
                    {
                        RPC_OnPlayerChooseCharacter(characterType);
                    }
                }

                GUILayout.EndHorizontal();
                
            
                // Draw other connected players and their ready status.
                foreach (PlayerRef playerRef in _networkManager.NetworkRunner.ActivePlayers)
                {
                    if (_networkManager.TryGetPlayerObject(playerRef, out Player player))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(player.PlayerRef.PlayerId.ToString());
                        GUILayout.Label(player.IsReadyToStartRace ? "Ready" : "Not Ready");
                        string chooseText = player.ChosenCharacter.ToString();
                        GUILayout.Label(chooseText);
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        if (LevelManager.Instance != null)
        {
            GUILayout.Label($"Chunks count: {LevelManager.Instance.GetChunks().Count}");
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
        _gui.CharacterChosenAction -= OnGuiChosenCharacter;
        _gui.ReadyToStartRaceChanged -= OnGuiChangedReadyToStartRace;
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
