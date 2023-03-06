using System;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OstSound : MonoBehaviour
{
    private AudioSource _ostAudioSource;

    private void Awake()
    {
        _ostAudioSource = GetComponent<AudioSource>();
        
        GameRunner gameRunner = GameRunner.Instance;
        Debug.Assert(gameRunner != null);
        if (gameRunner == null) return;

        gameRunner.GameStateHandlerSpawnedLocally += OnGameStateHandlerSpawnedLocally;
        gameRunner.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    private void OnGameStateHandlerSpawnedLocally()
    {
        GameStateHandler gameStateHandler = GameStateHandler.Instance;

        gameStateHandler.RaceStartedChanged += (raceStarted) =>
        {
            if(raceStarted)
            {
                Play();
            }
        };

        gameStateHandler.RaceFinished += (recordsList) =>
        {
            StopPlay();
        };
    }

    private void OnConnectionStatusChanged(ConnectionStatus prevStatus, ConnectionStatus status)
    {
        if (status == ConnectionStatus.NotInSession)
        {
            StopPlay();
        }
    }

    private void Play()
    {
        if (_ostAudioSource == null)
        {
            Debug.LogWarning($"{(nameof(_ostAudioSource))} is null. Can not play ost.");
        }
        else
        {
            _ostAudioSource.Play();
        }
    }

    private void StopPlay()
    {
        if (_ostAudioSource == null)
        {
            Debug.LogWarning($"{(nameof(_ostAudioSource))} is null. Can not stop play ost.");
        }
        else
        {
            _ostAudioSource.Stop();
        }
    }

    private void OnDestroy()
    {
        GameRunner gameRunner = GameRunner.Instance;
        if (gameRunner)
        {
            gameRunner.GameStateHandlerSpawnedLocally -= OnGameStateHandlerSpawnedLocally;
        }
    }
}
