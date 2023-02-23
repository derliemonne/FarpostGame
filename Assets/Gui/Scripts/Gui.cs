using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
    
public class Gui : MonoBehaviour
{
    public static Gui Instance { get; private set; }
    
    public event Action<int> ConnectToRoomWithId;
    public event Action StartHost;
    public event Action<bool> ReadyToStartRaceChanged;
    public event Action<CharacterType> CharacterChosenAction;

    public CharacterType ChosenCharacter { get; private set; }

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
    [SerializeField] private Button _readyButton;
    [Header("Leaders menu")] 
    [SerializeField] private GuiRecord _record1st;
    [SerializeField] private GuiRecord _record2nd;
    [SerializeField] private GuiRecord _record3rd;
    [SerializeField] private GuiRecord _record4th;
    [Header("MessageBoxes")]
    [SerializeField] private MessageBox _connectingMessageBox;
    [SerializeField] private MessageBox _badInputMessageBox;
    

    private List<RectTransform> _menus;
    private Dictionary<CharacterType, Animator> _characterAnimators;
    private List<GuiRecord> _guiRecords;
    private int _connectedPlayersCount;
    
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
        _guiRecords = new List<GuiRecord> { _record1st, _record2nd, _record3rd, _record4th };
        
        _mainMenuButtonExit.onClick.AddListener(OnMainMenuButtonExitClicked);
        _mainMenuButtonHost.onClick.AddListener(OnMainMenuButtonHostClicked);
        _mainMenuButtonConnect.onClick.AddListener(OnMainMenuButtonConnectClicked);
        _connectMenuButtonConnect.onClick.AddListener(OnConnectMenuButtonConnectClicked);
        _chooseFirsikButton.Button.onClick.AddListener(OnChooseFirsikButtonClicked);
        _choosePirsikButton.Button.onClick.AddListener(OnChoosePirsikButtonClicked);
        _chooseMarsikButton.Button.onClick.AddListener(OnChooseMarsikButtonClicked);
        _chooseGullButton.Button.onClick.AddListener(OnChooseGullButtonClicked);
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
        
        foreach (Button button in _buttonsToOpenMenu)
        {
            button.onClick.AddListener(OnButtonToOpenMainMenuClicked);
        }

        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        if (gameStateHandler == null)
        {
            Debug.LogError("GameStateHandler is null.");
        }
        gameStateHandler.ConnectionStatusChanged += OnConnectionStatusChanged;
        gameStateHandler.RaceStartedChanged += OnRaceStartedChanged;
        gameStateHandler.RaceFinished += OnRaceFinished;
    }
    
    private void Update()
    {
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        NetworkManager networkManager = NetworkManager.Instance;
        if (networkManager != null && gameStateHandler != null && gameStateHandler.Object != null && 
            gameStateHandler.ConnectionStatus == ConnectionStatus.InSession && !gameStateHandler.RaceStarted)
        {
            int connectedPlayersCount = networkManager.NetworkRunner.SessionInfo.PlayerCount;
            if (connectedPlayersCount != _connectedPlayersCount)
            {
                _playersConnectedCountText.text = 
                    $"В комнату вошли {connectedPlayersCount} игроков. Максимум {GameStateHandler.MaxPlayersInSession} игроков.";
            }
            _connectedPlayersCount = connectedPlayersCount;
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
    }

    private void OnMainMenuButtonHostClicked()
    {
        StartHost?.Invoke();
    }
    
    private void OnMainMenuButtonConnectClicked() => ShowMenu(_connectMenu);

    private void OnMainMenuButtonExitClicked() => Debug.Log("GUI: Exit game button.");

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
        SpotlightCharacter(CharacterType.Pirsik);
    }

    private void OnChooseGullButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Gull);
        SpotlightCharacter(CharacterType.Gull);
    }

    private void OnChooseMarsikButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Marsik);
        SpotlightCharacter(CharacterType.Marsik);
    }

    private void OnChooseFirsikButtonClicked()
    {
        CharacterChosenAction?.Invoke(CharacterType.Firsik);
        SpotlightCharacter(CharacterType.Firsik);
    }

    private void OnReadyButtonClicked()
    {
        ReadyToStartRaceChanged?.Invoke(true);
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
        if (raceStarted) ShowMenu(null);
    }

    private void OnRaceFinished(List<Record> records)
    {
        ShowMenu(_leadersMenu);
        records.Sort((r1, r2) => r1.Place.CompareTo(r2.Place));
        for (int i = 0; i < records.Count; i++)
        {
            _guiRecords[i].Record = records[i];
        }
    }

    private void OnDestroy()
    {
        _mainMenuButtonHost.onClick.RemoveAllListeners();
        // TODO: remove all other listeners
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
