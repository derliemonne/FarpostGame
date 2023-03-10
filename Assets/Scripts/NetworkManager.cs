using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;


public enum NetworkMessage
{
    ServerKickedRaceStarted
}


[RequireComponent(typeof(NetworkRunner))]
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public static Dictionary<NetworkMessage, string> NetworkMessages = new()
    {
        {
            NetworkMessage.ServerKickedRaceStarted,
            "В этой комнате игра уже началась. Попробуйте подключиться к другой."
        }
    };

    private NetworkRunner _networkRunner;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public bool IsPlayerHost(int playerId)
    {
        return playerId == NetworkRunner.SessionInfo.MaxPlayers - 1;
    }
    
    public NetworkRunner NetworkRunner
    {
        get 
        {
            if (_networkRunner == null)
            {
                _networkRunner = GetComponent<NetworkRunner>();
            }
            return _networkRunner;
        }
    }

    [CanBeNull, Pure]
    public Player GetPlayerObject(PlayerRef playerRef)
    {
        NetworkObject playerObject = NetworkRunner.GetPlayerObject(playerRef);
        if (playerObject == null) return null;
        if (playerObject.TryGetComponent(out Player player))
        {
            return player;
        }
        return null;
    }

    [CanBeNull, Pure]
    public Player GetLocalPlayer()
    {
        return GetPlayerObject(NetworkRunner.LocalPlayer);
    }

    [Pure]
    public bool TryGetPlayerObject(PlayerRef playerRef, out Player playerOut)
    {
        if (NetworkRunner.TryGetPlayerObject(playerRef, out NetworkObject playerObj) &&
            playerObj.TryGetComponent(out Player player))
        {
            playerOut = player;
            return true;
        }
        playerOut = null;
        return false;
    }

    [Pure, ItemNotNull]
    public IEnumerable<Player> GetActivePlayers()
    {
        return NetworkRunner.ActivePlayers
            .Select(playerRef => GetPlayerObject(playerRef))
            .Where(player => player != null);
    }

    public Player GetRandomActivePlayer()
    {
        List<Player> activePlayers = GetActivePlayers().ToList();
        int randomPlayerInd = Random.Range(0, activePlayers.Count());
        return activePlayers[randomPlayerInd];
    }

    public Player GetRandomActivePlayerWithException(int playerIdExcpet)
    {
        List<Player> activePlayers = NetworkRunner.ActivePlayers
            .Select(playerRef => GetPlayerObject(playerRef))
            .Where(player => player != null)
            .Where(player => player.Character.PlayerId != playerIdExcpet)
            .ToList();
        if(activePlayers.Count == 0)
        {
            Debug.LogError("Except player was the only player");
            return null;
        }
        else
        {
            int randomPlayerInd = Random.Range(0, activePlayers.Count());
            return activePlayers[randomPlayerInd];
        }
    }

    public IEnumerable<Player> GetAlivePlayers()
    {
        return NetworkRunner.ActivePlayers
            .Select(playerRef => GetPlayerObject(playerRef))
            .Where(player => player != null)
            .Where(player => player.Character.IsDead != true);
    }

    public Player GetRandomAlivePlayerWithException(int playerIdExcpet)
    {
        List<Player> activePlayers = NetworkRunner.ActivePlayers
            .Select(playerRef => GetPlayerObject(playerRef))
            .Where(player => player != null)
            .Where(player => player.Character.IsDead != true)
            .Where(player => player.Character.PlayerId != playerIdExcpet)
            .ToList();
        if (activePlayers.Count == 0)
        {
            Debug.LogError("Except player was the only alive player");
            return null;
        }
        else
        {
            int randomPlayerInd = Random.Range(0, activePlayers.Count());
            return activePlayers[randomPlayerInd];
        }
    }

    public void Despawn(NetworkObject networkObject, float seconds, bool allowPredicted=false)
    {
        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            NetworkRunner.Despawn(networkObject, allowPredicted);  
        }
        StartCoroutine(Wait(seconds));
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
