using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPro.EditorUtilities;

public class Gui : MonoBehaviour
{
    public static Gui Instance { get; private set; }
    
    public event Action<int> ConnectToRoomWithId;
    public event Action StartHost;

    [Header("Menus")]
    [SerializeField] private RectTransform _mainMenu;
    [SerializeField] private RectTransform _connectMenu;
    [SerializeField] private RectTransform _roomMenu;
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
    [Header("MessageBoxes")]
    [SerializeField] private MessageBox _connectingMessageBox;
    [SerializeField] private MessageBox _badInputMessageBox;
    

    private List<RectTransform> _menus;
    private int _connectedPlayersCount;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _menus = new List<RectTransform> { _mainMenu, _connectMenu, _roomMenu };
        
        _mainMenuButtonExit.onClick.AddListener(OnMainMenuButtonExitClicked);
        _mainMenuButtonHost.onClick.AddListener(OnMainMenuButtonHostClicked);
        _mainMenuButtonConnect.onClick.AddListener(OnMainMenuButtonConnectClicked);
        
        _connectMenuButtonConnect.onClick.AddListener(OnConnectMenuButtonConnectClicked);
        
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
    }

    private void Update()
    {
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        NetworkManager networkManager = NetworkManager.Instance;
        if (networkManager != null && gameStateHandler != null &&
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

    public void A()
    {
        print("A");
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

    private void OnMainMenuButtonHostClicked()
    {
        StartHost?.Invoke();
        ShowMenu(_roomMenu);
    }
    
    private void OnMainMenuButtonConnectClicked() => ShowMenu(_connectMenu);

    private void OnMainMenuButtonExitClicked() => Debug.Log("GUI: Exit game button.");

    private void OnConnectMenuButtonConnectClicked()
    {
        if (int.TryParse(_inputRoomId.text, out int roomId))
        {
            ConnectToRoomWithId?.Invoke(roomId);
            _connectingMessageBox.SetActive(true);
        }
        else
        {
            _badInputMessageBox.SetActive(true);
        }
    }

    private void OnButtonToOpenMainMenuClicked() => ShowMenu(_mainMenu);

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
    }

    private void OnRaceStartedChanged(bool raceStarted)
    {
        if (raceStarted) ShowMenu(null);
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
