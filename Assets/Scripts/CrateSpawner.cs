using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Need to be only on host machine.
/// </summary>
public class CrateSpawner : NetworkBehaviour
{ 
    private float Altitude { get; set; }

    public const float CrateSize = 1.28f;
    public const float CrateHalfSize = CrateSize / 2;

    /// <summary>
    /// How many units above the topChunk the boxes will be spawned.
    /// </summary>
    [SerializeField] private float _altitudeShiftAboveTopChunk = Chunk.ChunkHalfHeight;
    [SerializeField] private float _cratesSpawnInterval = 4f;
    [SerializeField] private List<Crate> _cratePrefabs;
    
    private bool _isSpawningCrates;
    private float _lastTimeCrateSpawned;

    public override void Spawned()
    {
        GameRunner gameRunner = GameRunner.Instance;
        Debug.Assert(gameRunner != null);
        if (gameRunner != null)
        {
            gameRunner.GameStateHandlerSpawnedLocally += OnGameStateHandlerSpawnedLocally;
        }
    }

    private void OnGameStateHandlerSpawnedLocally()
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

        List<Chunk> chunks = levelManager.GetChunks();
        if (chunks.Count == 0) return;
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
        _isSpawningCrates = raceStarted;
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
        if (!Runner.IsServer) return;
        
        if (!_isSpawningCrates) return;
        float timeNow = Time.time;
        if (timeNow - _lastTimeCrateSpawned > _cratesSpawnInterval)
        {
            Vector3 cratePos = new(Random.Range(-Chunk.ChunkHalfWidth + CrateHalfSize, Chunk.ChunkHalfWidth - CrateHalfSize), Altitude, 0);
            Crate cratePrefab = _cratePrefabs[Random.Range(0, _cratePrefabs.Count)];
            NetworkManager.Instance.NetworkRunner.Spawn(cratePrefab, cratePos);
            _lastTimeCrateSpawned = timeNow;
        }
    }

    private void OnDestroy()
    {
        GameRunner gameRunner = GameRunner.Instance;
        if (gameRunner)
        {
            gameRunner.GameStateHandlerSpawnedLocally -= OnGameStateHandlerSpawnedLocally;
        }
        
        GameStateHandler gameStateHandler = GameStateHandler.Instance;
        if (gameStateHandler != null)
        {
            gameStateHandler.RaceStartedChanged -= OnRaceStartedChanged;
        }
    }
}
