using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Need to be only on host machine.
/// </summary>
public class CrateSpawner : MonoBehaviour
{
    public bool IsSpawningBoxes { get; private set; }

    private float Altitude { get; set; }

    /// <summary>
    /// How many units above the topChunk the boxes will be spawned.
    /// </summary>
    [SerializeField] private float _altitudeShiftAboveTopChunk = Chunk.ChunkHalfHeight;
    [SerializeField] private float _cratesPerSecond = 0.4f;
    [SerializeField] private List<Crate> _cratePrefabs;

    private void Awake()
    {
        GameStateHandler.Instance.RaceStartedChanged += OnRaceStartedChanged;
    }

    private void UpdateAltitude()
    {
        LevelManager levelManager = LevelManager.Instance;
        if (levelManager == null)
        {
            Debug.LogError("LevelManager is null");
            return;
        }

        Chunk topChunk = levelManager.GetChunks()[^1];
        if (topChunk == null)
        {
            Debug.LogError("TopChunk is null");
            return;
        }
        
        Altitude = topChunk.Altitude + _altitudeShiftAboveTopChunk;
    }

    private void OnRaceStartedChanged(bool raceStarted)
    {
        IsSpawningBoxes = raceStarted;
        LevelManager levelManager = LevelManager.Instance;
        if (raceStarted)
        {
            if (levelManager == null)
            {
                Debug.LogError("LevelManger is null");
                return;
            }
            levelManager.ChunksChanged += OnChunksChanged;
        }
        else if (levelManager != null)
        {
            levelManager.ChunksChanged -= OnChunksChanged;
        }
        UpdateAltitude();
    }

    private void OnChunksChanged()
    {
        UpdateAltitude();
    }

    private void FixedUpdate()
    {
        if (!IsSpawningBoxes) return;
        float boxesPerTick = _cratesPerSecond * Time.fixedDeltaTime;
        if (Random.value <= boxesPerTick)
        {
            Vector3 cratePos = new(Random.Range(-Chunk.ChunkHalfWidth, Chunk.ChunkHalfWidth), Altitude, 0);
            Crate cratePrefab = _cratePrefabs[Random.Range(0, _cratePrefabs.Count)];
            NetworkManager.Instance.NetworkRunner.Spawn(cratePrefab, cratePos);
        }
    }

    private void OnDestroy()
    {
        if (GameStateHandler.Instance != null)
            GameStateHandler.Instance.RaceStartedChanged -= OnRaceStartedChanged;
    }
}
