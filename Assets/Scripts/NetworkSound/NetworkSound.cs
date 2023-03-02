using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(AudioSource))]
public abstract class NetworkSound : NetworkBehaviour
{
    private AudioSource _audioSource;
    private Dictionary<string, AudioClip> _soundMap; 

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _soundMap = MakeSoundMap();
    }

    /// <summary>
    /// Server-only.
    /// </summary>
    protected void PlayToOne(PlayerRef playerRef, string soundName)
    {
        if(!Runner.IsServer)
        {
            Debug.LogError("Server-only method execute on client");
            return;
        }
        Rpc_PlayToOne(playerRef, soundName);
    }

    /// <summary>
    /// Server-only.
    /// </summary>
    protected void PlayToAll(string soundName)
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("Server-only method execute on client");
            return;
        }
        Rpc_PlayToAll(soundName);
    }

    protected abstract Dictionary<string, AudioClip> MakeSoundMap();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_PlayToOne([RpcTarget] PlayerRef playerRef, string soundName)
    {
        _audioSource.clip = _soundMap[soundName];
        _audioSource.Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_PlayToAll(string soundName)
    {
        _audioSource.clip = _soundMap[soundName];
        _audioSource.Play();
    }
}
