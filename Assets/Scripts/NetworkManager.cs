using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(NetworkRunner))]
public class NetworkManager : MonoBehaviour
{
    private NetworkRunner _networkRunner;

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

    [CanBeNull]
    public Player GetPlayerObject(PlayerRef playerRef)
    {
        return NetworkRunner.GetPlayerObject(playerRef).GetComponent<Player>();
    }

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

    public IEnumerable<Player> GetActivePlayers()
    {
        return NetworkRunner.ActivePlayers.
            Select(playerRef => GetPlayerObject(playerRef)).
            Where(player => player != null);
    }
}
