using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CanvasGroup = UnityEngine.CanvasGroup;

public class Gui : MonoBehaviour
{
    public static Gui Instance { get; private set; }
    
    public event Action<int> ConnectToRoomWithId;
    public event Action StartHost;
    public event Action<bool> ReadyToStartRaceChanged;
    public event Action<CharacterType> CharacterChosenAction;
    public event Action LeavedSession;

    public CharacterType ChosenCharacter { get; private set; }
    /*
    public InputButton UpInputButton => _upInputButton;
    public InputButton LeftInputButton => _leftInputButton;
    public InputButton RightInputButton => _rightInputButton;

    public InputButton PushPlatformButton => _pushPlatformButton;
    */
    public MoveController MoveController => _moveController;

    [SerializeField] private bool _drawDebugInfo;
    [Header("Menus")]
    [SerializeField] private RectTransform _mainMenu;
    [SerializeField] private RectTransform _connectMenu;
    [SerializeField] private RectTransform _roomMenu;
    [SerializeField] private RectTransform _leadersMenu;
    [Header("Buttons in main menu")] 
    [SerializeField] private Button _mainMenuButtonExit;
    [SerializeField] private Button _mainMenuButtonHost;
    [SerializeField] private Button _mainMenuButtonConnect;
    [Header("Buttons that open main menu.")]
    [SerializeField] private List<Button> _buttonsToOpenMenu;
    [Header("Connect menu")] 
    [SerializeField] private TMP_InputField _inputRoomId;
    [SerializeField] private Button _connectMenuButtonConnect;
    [SerializeField] private Button _abortConnectToRoomButton;
    [Header("Room menu")] 
    [SerializeField] private TMP_Text _roomIdText;
    [SerializeField] private TMP_Text _playersConnectedCountText;
    [SerializeField] private Animator _firsikAnimator;
    [SerializeField] private Animator _pirsikAnimator;
    [SerializeField] private Animator _marsikAnimator;
    [SerializeField] private Animator _gullAnimator;
    [SerializeField] private ChooseCharacterButton _chooseFirsikButton;
    [SerializeField] private ChooseCharacterButton _choosePirsikButton;
    [SerializeField] private ChooseCharacterButton _chooseMarsikButton;
    [SerializeField] private ChooseCharacterButton _chooseGullButton;
    [SerializeField] private Button _leaveSessionButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private SerializableDictionary<CharacterType, CanvasGroup> _characterCanvasGroups;
    [SerializeField, Range(0, 1)] private float _unchosenCharacterSpriteAlphaChannel = 0.8f;
    [Header("Leaders menu")] 
    [SerializeField] private Button _leadersMenuLeaveSessionButton;
    [SerializeField] private GuiRecord _record1st;
    [SerializeField] private GuiRecord _record2nd;
    [SerializeField] private GuiRecord _record3rd;
    [SerializeField] private GuiRecord _record4th;
    [Header("MessageBoxes")]
    [SerializeField] private MessageBox _connectingMessageBox;
    [SerializeField] private MessageBox _badInputMessageBox;
    [SerializeField] private MessageBox _messageBox;
    /*
    [Header("Input")]
    [SerializeField] private GameObject _controlsParent;
    [SerializeField] private InputButton _leftInputButton;
    [SerializeField] private InputButton _rightInputButton;
    [SerializeField] private InputButton _upInputButton;
    [SerializeField] private InputButtonUp _upInputButtonChanger;
    [SerializeField] private InputButton _pushPlatformButton;
    [SerializeField] private PushPlatformButton _pushPlatformButtonChanger;
    */
    [Header("New input")]
    [SerializeField] private GameObject _controlsParent;
    [SerializeField] private MoveController _moveController;
    [SerializeField] private InputButtonUp _upInputButtonChanger;
    [SerializeField] private PushPlatformButton _pushPlatformButtonChanger;
    [SerializeField] private Button _exitRaceButton;

    private List<RectTransform> _menus;
    private Dictionary<CharacterType, Animator> _characterAnimators;
    private Dictionary<CharacterType, ChooseCharacterButton> _charactersChooseButtons;
    private List<GuiRecord> _guiRecords;
    private int _connectedPlayersCount;

    public void ShowMessageBox(string message)
    {
        if (_messageBox == null)
        {
            Debug.LogError($"{nameof(_messageBox)} is null.");
            return;
        }
        
        _messageBox.SetText(message);
        _messageBox.SetActive(true);
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _menus = new List<RectTransform> { _mainMenu, _connectMenu, _roomMenu, _leadersMenu };
        _characterAnimators = new Dictionary<CharacterType, Animator>
        {
            { CharacterType.Pirsik, _pirsikAnimator },
            { CharacterType.Firsik, _firsikAnimator },
            { CharacterType.Marsik, _marsikAnimator },
            { CharacterType.Gull, _gullAnimator }
        };
        _charactersChooseButtons = new Dictionary<CharacterType, ChooseCharacterButton>
        {
            { CharacterType.Pirsik, _choosePirsikButton },
            { CharacterType.Firsik, _chooseFirsikButton },
            { CharacterType.Marsik, _chooseMarsikButton },
            { CharacterType.Gull, _chooseGullButton }
        };
            
        _guiRecords = new List<GuiRecord> { _record1st, _record2nd, _record3rd, _record4th };
        
        _mainMenuButtonExit.onClick.AddListener(OnMainMenuButtonExitClicked);
        _mainMenuButtonHost.onClick.AddListener(OnMainMenuButtonHostClicked);
        _mainMenuButtonConnect.onClick.AddListener(OnMainMenuButtonConnectClicked);
        _connectMenuButtonConnect.onClick.AddListener(OnConnectMenuButtonConnectClicked);
        _chooseFirsikButton.Button.onClick.AddListener(OnChooseFirsikButtonClicked);
        _choosePirsikButton.Button.onClick.AddListener(OnChoosePirsikButtonClicked);
        _chooseMarsikButton.Button.onClick.AddListener(OnChooseMarsikButtonClicked);
        _chooseGullButton.Button.onClick.AddListener(OnChooseGullButtonClicked);
        _leaveSessionButton.onClick.AddListener(OnLeaveSessionButtonClicked);
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
        _leadersMenuLeaveSessionButton.onClick.AddListener(OnLeadersMenuLeaveSessionButtonClicked);
        _exitRaceButton.onClick.AddListener(OnExitRaceButtonClicked);
        _abortConnectToRoomButton.onClick.AddListener(OnAbortConnectToRoomButtonClicked);

        foreach (Button button in _buttonsToOpenMenu)
        {
            button.onClick.AddListener(OnButtonToOpenMainMenuClicked);
        }
        
        GameRunner gameRunner = GameRunner.Instance;
        Debug.Assert(gameRunner != null);
        if (gameRunner != null)
        {
            gameRunner.GameStateHandlerSpawnedLocally += OnGameStateHandlerSpawnedLocally;
            gameRunner.ConnectionStatusChanged += OnConnectionStatusChanged;
        }
    }

    private void OnGameStateHandlerSpawnedLocally()
    {
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        if (gameStateHandler == null)
        {
            Debug.LogError("GameStateHandler is null.");
        }
        gameStateHandler.RaceStartedChanged += OnRaceStartedChanged;
        gameStateHandler.RaceFinished += OnRaceFinished;

        Player player = NetworkManager.Instance.GetLocalPlayer();
        Debug.Assert(player != null);
        if (player != null)
        {
            player.ChosenCharacterChanged += OnLocalPlayerChosenCharacterChanged;
        }
    }
    
    private void Update()
    {
        GameRunner gameRunner = GameRunner.Instance;
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        NetworkManager networkManager = NetworkManager.Instance;
        
        if (gameRunner == null || networkManager == null || gameStateHandler == null || gameStateHandler.Object == null)
            return;
        if (gameRunner.ConnectionStatus != ConnectionStatus.InSession) return;
        
        if (gameStateHandler.RaceStarted)
        {
            Character localCharacter = gameStateHandler.LocalCharacter;
            if (localCharacter != null && localCharacter.Object != null)
            {
                bool isDoubleJumpActive = localCharacter is GullScript gull && gull.DoubleJump;
                _upInputButtonChanger.SetButtonImage(localCharacter.CanJump, isDoubleJumpActive);
                _pushPlatformButtonChanger.SetButtonImage(localCharacter._canPushPlatform);
            }
        }
        else
        {
            int connectedPlayersCount = networkManager.NetworkRunner.SessionInfo.PlayerCount;
            if (connectedPlayersCount != _connectedPlayersCount)
            {
                _playersConnectedCountText.text = 
                    $"В комнату вошли {connectedPlayersCount} игроков. Максимум {GameStateHandler.MaxPlayersInSession} игроков.";
            }
            _connectedPlayersCount = connectedPlayersCount;
            
            Player[] players = NetworkManager.Instance.GetActivePlayers().ToArray();
            foreach (CharacterType character in Enum.GetValues(typeof(CharacterType)))
            {
                if (!_charactersChooseButtons.ContainsKey(character))
                {
                    continue;
                }
                // If no one have chosen character then it will be able to be chosen.
                bool canBeChosen = players.All(player => player.ChosenCharacter != character);
                _charactersChooseButtons[character].Button.interactable = canBeChosen;
            }
        }
    }
    
    /// <summary>
    /// Pass null if you want to close all menus.
    /// </summary>
    private void ShowMenu(RectTransform menuToShow)
    {
        foreach (RectTransform menu in _menus)
        {
            menu.gameObject.SetActive(false);
        }
        if (menuToShow != null) menuToShow.gameObject.SetActive(true);
    }
    
    private void SpotlightCharacter(CharacterType character)
    {
        foreach (Animator characterAnimator in _characterAnimators.Values)
        {
            characterAnimator.enabled = false;
        }
        if (_characterAnimators.TryGetValue(character, out Animator animator))
        {
            animator.enabled = true;
        }

        Debug.Assert(_characterCanvasGroups.Values.Count == 4);
        foreach (CanvasGroup canvasGroup in _characterCanvasGroups.Values)
        {
            canvasGroup.alpha = _unchosenCharacterSpriteAlphaChannel;
        }
        foreach (CharacterType characterType in _characterCanvasGroups.Keys)
        {
            CanvasGroup canvasGroup = _characterCanvasGroups[characterType];
            canvasGroup.alpha = _unchosenCharacterSpriteAlphaChannel;
        }
        if (_characterCanvasGroups.ContainsKey(character))
        {
            _characterCanvasGroups[character].alpha = 1;
        }
    }

    private void LeaveSession()
    {
        LeavedSession?.Invoke();
        _controlsParent.SetActive(false);
    }

    private void OnMainMenuButtonHostClicked()
    {
        StartHost?.Invoke();
    }
    
    private void OnMainMenuButtonConnectClicked() => ShowMenu(_connectMenu);

    private void OnMainMenuButtonExitClicked()
    {
        Application.Quit();
    }

    private void OnConnectMenuButtonConnectClicked()
    {
        if (int.TryParse(_inputRoomId.text, out int roomId))
        {
            ConnectToRoomWithId?.Invoke(roomId);
        }
        else
        {
            _badInputMessageBox.SetActive(true);
        }
    }

    private void OnButtonToOpenMainMenuClicked() => ShowMenu(_mainMenu);
    
    private void OnChoosePirsikButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Pirsik);
    }

    private void OnChooseGullButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Gull);
    }

    private void OnChooseMarsikButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Marsik);
    }

    private void OnChooseFirsikButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Firsik);
    }

    private void OnLeaveSessionButtonClicked()
    {
        LeaveSession();
    }

    private void OnReadyButtonClicked()
    {
        ReadyToStartRaceChanged?.Invoke(true);
    }

    private void OnLeadersMenuLeaveSessionButtonClicked()
    {
        LeaveSession();
    }

    private void OnExitRaceButtonClicked()
    {
        LeaveSession();
    }

    private void OnAbortConnectToRoomButtonClicked()
    {
        LeaveSession();
    }

    private void OnLocalPlayerChosenCharacterChanged(Player localPlayer, CharacterType characterType)
    {
        GameRunner gameRunner = GameRunner.Instance;
        Debug.Assert(gameRunner != null);
        if (gameRunner == null) return;
        
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        if (gameStateHandler == null) return;

        if (!gameStateHandler.RaceStarted)
        {
            SpotlightCharacter(characterType);
        }
    }

    private void OnConnectionStatusChanged(ConnectionStatus previousStatus, ConnectionStatus newStatus)
    {
        if (newStatus == ConnectionStatus.NotInSession) ShowMenu(_mainMenu);
        else if (newStatus == ConnectionStatus.InSession)
        {
            ShowMenu(_roomMenu);
            NetworkManager networkManager = NetworkManager.Instance;
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager is null");
                return;
            }
            _roomIdText.text = $"Номер комнаты: {NetworkManager.Instance.NetworkRunner.SessionInfo.Name}";
            
            Player localPlayer = networkManager.GetLocalPlayer();
            if (localPlayer != null)
            {
                SpotlightCharacter(localPlayer.ChosenCharacter);
            }
        }
        if (newStatus == ConnectionStatus.ConnectingToSession)
        {
            _connectingMessageBox.SetActive(true);
        }
        if (previousStatus == ConnectionStatus.ConnectingToSession && newStatus != ConnectionStatus.ConnectingToSession)
        {
            _connectingMessageBox.SetActive(false);   
        }
    }

    private void OnRaceStartedChanged(bool raceStarted)
    {
        if (raceStarted)
        {
            ShowMenu(null);
            _controlsParent.SetActive(true);
        }
    }

    private void OnRaceFinished(List<Record> records)
    {
        ShowMenu(_leadersMenu);
        records.Sort((r1, r2) => r1.Place.CompareTo(r2.Place));
        for (int i = 0; i < records.Count; i++)
        {
            _guiRecords[i].Record = records[i];
        }
        _controlsParent.SetActive(false);
    }

    private void OnDestroy()
    {
        _mainMenuButtonHost.onClick.RemoveAllListeners();
        // TODO: remove all other listeners
        
        GameRunner gameRunner = GameRunner.Instance;
        if (gameRunner)
        {
            gameRunner.GameStateHandlerSpawnedLocally -= OnGameStateHandlerSpawnedLocally;
            gameRunner.ConnectionStatusChanged -= OnConnectionStatusChanged;
        }
        
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        if (gameStateHandler)
        {
            gameStateHandler.RaceStartedChanged -= OnRaceStartedChanged;
            gameStateHandler.RaceFinished -= OnRaceFinished;
        }
    }

    private void OnGUI()
    {
        if (!_drawDebugInfo) return;
        GameRunner gameRunner = GameRunner.Instance;
        if (gameRunner == null) return;
        // Scale GUI for phones.
        float scaleFactor = 4;
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(scaleFactor, scaleFactor, 1));
        GUILayout.Label($"ConnectionStatus: {gameRunner.ConnectionStatus}");
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
