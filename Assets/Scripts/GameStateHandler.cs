using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fusion;
using IngameDebugConsole;
using UnityEngine;


public class GameStateHandler : NetworkBehaviour
{
    public const int MaxPlayersInSession = 4;
    
    public static GameStateHandler Instance { get; private set; }


    public event Action<bool> RaceStartedChanged;
    public event Action<List<Record>> RaceFinished;
    
    
    [Networked(OnChanged = nameof(OnRaceStartedChanged))] 
    public NetworkBool RaceStarted { get; set; }
    
    [Networked(OnChanged = nameof(OnRaceFinishedChanged))]
    public NetworkBool IsRaceFinished { get; private set; }
    
    public Character LocalCharacter { get; private set; }
    
    public ReadOnlyCollection<Character> CharacterPrefabs => _characterPrefabs.Values.ToList().AsReadOnly();

    [SerializeField] private SerializableDictionary<CharacterType, Character> _characterPrefabs;
    [SerializeField] private Darkness _darknessPrefab;
    [SerializeField] private Vector2 _startCharacterPosition = new(0, 0);

    private string _inputSessionName;
    private float _prevInputMoveY;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        Gui gui = Gui.Instance;
        if (gui == null)
        {
            Debug.LogError("Gui is null");
        }
        else
        {
            gui.CharacterChosenAction += OnGuiChosenCharacter;
            gui.ReadyToStartRaceChanged += OnGuiChangedReadyToStartRace;
        }
    }

    public override void Spawned()
    {
        GameRunner gameRunner = GameRunner.Instance;
        
        Debug.Assert(gameRunner != null);
        if (gameRunner == null) return;
        
        gameRunner.NotifyGameStateHandlerSpawnedLocally();
    }

    public static void OnRaceStartedChanged(Changed<GameStateHandler> changed)
    {
        GameStateHandler gameStateHandler = changed.Behaviour;
        gameStateHandler.RaceStartedChanged?.Invoke(gameStateHandler.RaceStarted);

        if (gameStateHandler.RaceStarted)
        {
            PlayerRef localPlayerRef = NetworkManager.Instance.NetworkRunner.LocalPlayer;
            Player player = NetworkManager.Instance.GetPlayerObject(localPlayerRef);
            if (player != null) gameStateHandler.LocalCharacter = player.Character;
        }
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void Rpc_OnPlayerChangedReadyToStartRace(bool readyToStart, RpcInfo rpcInfo=default)
    {
        PlayerRef playerRef = rpcInfo.Source;
        Debug.Log($"> RPC on {Runner.LocalPlayer.PlayerId} from {playerRef.PlayerId}.");
        if (NetworkManager.Instance.TryGetPlayerObject(playerRef, out Player player))
        {
            player.IsReadyToStartRace = readyToStart;
        }

        bool allPlayersAreReady = Runner.ActivePlayers
            .Select(playerRef => NetworkManager.Instance.GetPlayerObject(playerRef))
            .All(player => player != null && player.IsReadyToStartRace);
        if (allPlayersAreReady)
        {
            RaceStarted = true;
            HostStartRace();
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
        CameraScript cameraScript = CameraScript.Instance;
        if (cameraScript == null)
        {
            Debug.LogError($"{nameof(cameraScript)} is null.");
        }
        else
        {
            cameraScript.BindPlayer(character.transform, character.GetComponent<Rigidbody2D>(), character);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_BindPlayerSound([RpcTarget] PlayerRef player, Character character)
    {
        PlayerSound playerSound = PlayerSound.Instance;
        if (playerSound == null)
        {
            Debug.LogWarning($"{nameof(playerSound)} is null");
        }
        else
        {
            playerSound.BindPlayer(character);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_MakePlayerObserver([RpcTarget] PlayerRef player, Character character)
    {
        ObserverScript.Instance.BindToCharacter(character);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_SendMessageToClient([RpcTarget] PlayerRef playerRef, NetworkMessage networkMessage)
    {
        string message = NetworkManager.NetworkMessages.GetValueOrDefault(networkMessage, "undefined network message");
        Debug.Log($"> NetworkMessage: {message}.");

        Gui gui = Gui.Instance;
        if (gui == null)
        {
            Debug.LogError($"{nameof(gui)} is null");
        }
        else
        {
            gui.ShowMessageBox(message);
        }
    }

    public void DebugConsoleCommand_SendMessageToClient(int playerId, NetworkMessage message)
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("This is server-only command.");
            return;
        }
        Rpc_SendMessageToClient(playerId, message);
    }

    private void Start()
    {
        DebugLogConsole.AddCommandInstance(
            "send",
            "sends message from host to client via rpc",
            nameof(DebugConsoleCommand_SendMessageToClient),
            this);
    }

    private void HostStartRace()
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("Host method executes on client.");
            return;
        }
        foreach (Player player in NetworkManager.Instance.GetActivePlayers())
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
            Character character = Runner.Spawn(chosenPrefab, _startCharacterPosition, inputAuthority: player.PlayerRef);
            if (character == null)
            {
                Debug.LogError("Character prefab is invalid.");
            }
            character.SetPlayerId(player.PlayerRef.PlayerId);
            Rpc_MakePlayerObserver(player.PlayerRef.PlayerId, character);      
            player.Character = character;
            Rpc_BindCamera(player.PlayerRef, character);
            character.Died += OnCharacterDied;
            
            Rpc_BindPlayerSound(player.PlayerRef, character);
            PlayerSound playerSound = PlayerSound.Instance;
            if (playerSound == null)
            {
                Debug.LogWarning($"{nameof(playerSound)} is null");
            }
            else
            {
                character.BindPlayerSound(PlayerSound.Instance);
            }
        }

        Darkness darkness = Runner.Spawn(_darknessPrefab);
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

    private void OnGuiChosenCharacter(CharacterType characterType)
    {
        if (Object != null)
        {
            RPC_OnPlayerChooseCharacter(characterType);
        }
    }

    private void OnGuiChangedReadyToStartRace(bool readyToStartRace)
    {
        Rpc_OnPlayerChangedReadyToStartRace(readyToStartRace);
    }
    
    private void OnDestroy()
    {
        Gui gui = Gui.Instance;
        if (gui == null)
        {
            return;
        }
        gui.CharacterChosenAction -= OnGuiChosenCharacter;
        gui.ReadyToStartRaceChanged -= OnGuiChangedReadyToStartRace;
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
