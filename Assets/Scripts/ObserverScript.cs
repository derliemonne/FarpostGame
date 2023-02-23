using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CameraScript))]
public class ObserverScript : MonoBehaviour
{ 
    public static ObserverScript Instance { get; private set; }

    [SerializeField] private float _changePlayerDuration;

    private CameraScript _cameraScript;
    private Coroutine _playerChangeCoroutine;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("Instance already exists");
            Destroy(gameObject);
        }
        Instance = this;
        _cameraScript = GetComponent<CameraScript>();
    }

    private void Start()
    {
        GameStateHandler.Instance.RaceFinished += EndObserv;
    }

    public void StartObserv(Character character, float altitude)
    {
        if (NetworkManager.Instance.GetAlivePlayers().Count() > 0)
        {
            //print(NetworkManager.Instance.GetActivePlayers().Count());
            ChangePlayer();
            _playerChangeCoroutine = StartCoroutine(ChangePlayerCoroutine());
        }  
    }

    public void EndObserv(List<Record> records)
    {
        if(_playerChangeCoroutine != null )
        {
            StopCoroutine(_playerChangeCoroutine);
        }     
    }

    private IEnumerator ChangePlayerCoroutine()
    {
        yield return new WaitForSeconds(_changePlayerDuration);
        ChangePlayer();
        _playerChangeCoroutine = StartCoroutine(ChangePlayerCoroutine());
    }

    private void ChangePlayer()
    {
        Player randomPlayer = FindRandomPlayer();
        if(randomPlayer == null )
        {
            Debug.LogError("No players left");
        }
        _cameraScript.BindPlayer(randomPlayer.Character.transform, randomPlayer.Character.GetComponent<Rigidbody2D>(), randomPlayer.Character);
    }

    private Player FindRandomPlayer()
    {
        List<Player> activePlayer = NetworkManager.Instance.GetAlivePlayers().ToList();
        if(activePlayer.Count == 0)
        {
            Debug.LogError("No players left");
            return null;
        }
        int activePlayersCount = activePlayer.Count;
        int randomPlayerIndex = Random.Range(0, activePlayersCount);
        return activePlayer[randomPlayerIndex];
    }
}
