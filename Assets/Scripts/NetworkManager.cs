using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(NetworkRunner))]
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    
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
        return NetworkRunner.GetPlayerObject(playerRef).GetComponent<Player>();
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

    [Pure]
    public IEnumerable<Player> GetActivePlayers()
    {
        return NetworkRunner.ActivePlayers
            .Select(playerRef => GetPlayerObject(playerRef))
            .Where(player => player != null);
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
