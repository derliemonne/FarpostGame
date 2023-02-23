using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : NetworkBehaviour
{
    private const int MaxChunksInNetworkList = 16;
    
    public static LevelManager Instance { get; private set; }

    public int ChunksCount => _chunkPrefabs.Count;

    public event Action ChunksChanged;
    
    [Networked]
    public NetworkBool IsSpawningChunksAutomatically { get; private set; }
    
    [SerializeField] private List<Chunk> _chunkPrefabs;
    [SerializeField] private float _firstChunkAltitude = Chunk.ChunkHalfHeight;
    
    [Networked]
    private ref ChunksList Chunks => ref MakeRef<ChunksList>();
    
    public override void Spawned()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        IsSpawningChunksAutomatically = GameStateHandler.Instance.RaceStarted;
        GameStateHandler.Instance.RaceStartedChanged += OnRaceStartedChanged;
    }

    public void SpawnChunkOnTop()
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("Server-only method executes not on server!");
            return;
        }
        if (Chunks.Count == MaxChunksInNetworkList)
        {
            Debug.LogError("Unable to add more chunks to NetworkArray. No space.");
            return;
        }
        Chunk topChunk = GetChunk(Chunks.TopChunk);
        float newChunkAltitude = topChunk == null ? _firstChunkAltitude : topChunk.Altitude + Chunk.ChunkHeight;
        Vector3 newChunkPosition = new(0, newChunkAltitude, 0);
        Debug.Log($"SpawnChunk at altitude {newChunkPosition.y}");
        Chunk chunk = NetworkManager.Instance.NetworkRunner.Spawn(_chunkPrefabs[Random.Range(0, ChunksCount)], newChunkPosition);
        if (chunk == null)
        {
            Debug.LogError("Chunk prefab is invalid.");
            return;
        }
        Chunks.AddChunkToTop(chunk);
        ChunksChanged?.Invoke();
    }

    public void DespawnChunkFromBottom()
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("Server-only method executes not on server!");
            return;
        }
        if (Chunks.Count == 0) return;
        Chunk bottomChunk = GetChunk(Chunks.BottomChunk);
        if (bottomChunk == null) return;
        NetworkManager.Instance.NetworkRunner.Despawn(bottomChunk.Object);
        Chunks.RemoveBottomChunk();
        ChunksChanged?.Invoke();
    }

    /// <summary>
    /// Returns chunks from bottom to top.
    /// </summary>
    [ItemCanBeNull]
    public List<Chunk> GetChunks()
    {
        List<Chunk> chunks = new();
        for (int i = 0; i < Chunks.Count; i++)
        {
            chunks.Add(GetChunk(Chunks.Chunks[i]));
        }
        return chunks;
    }

    public List<SpawnPoint> GetAllSpawnPoints()
    {
        List<SpawnPoint> spawnPointsList = new List<SpawnPoint>();

        foreach(Chunk chunk in GetChunks())
        {
            spawnPointsList.AddRange(chunk.SortedSpawnPoints);
        }

        return spawnPointsList;
    }

    private void OnRaceStartedChanged(bool raceStarted)
    {
        IsSpawningChunksAutomatically = raceStarted;
    }

    private Chunk GetChunk(NetworkBehaviourId networkBehaviourId)
    {
        if (networkBehaviourId == default) return null;
        return Runner.TryFindBehaviour(networkBehaviourId, out Chunk chunk) ? chunk : null;
    }
    
    private void FixedUpdate()
    {
        if (!Runner.IsServer || !IsSpawningChunksAutomatically) return;
        if (Chunks.Count == 0 && (Darkness.Instance == null || Darkness.Instance.Altitude < Chunk.ChunkHeight))
        {
            SpawnChunkOnTop();
            return;
        }
        Chunk topChunk = GetChunk(Chunks.TopChunk);
        if (topChunk == null) return;
        float? charactersHighestAltitude = GetCharactersHighestAltitude();
        if (!charactersHighestAltitude.HasValue) return;
        if (topChunk.IsAltitudeFitsInChunk(charactersHighestAltitude.Value) && !DarknessAbsorbedChunk(topChunk))
        {
            SpawnChunkOnTop();
        }
        
        while (Chunks.Count != 0)
        {
            Chunk bottomChunk = GetChunk(Chunks.BottomChunk);
            if (bottomChunk == null) return;
            if (DarknessAbsorbedChunk(bottomChunk))
            {
                DespawnChunkFromBottom();
            }
            else
            {
                break;
            }
        }
    }

    private static bool DarknessAbsorbedChunk(Chunk chunk)
    {
        if (Darkness.Instance == null)
        {
            Debug.LogError("Darkness instance is null.");
        }
        return chunk.Altitude + Chunk.ChunkHeight < Darkness.Instance.Altitude;
    }

    [Pure]
    private float? GetCharactersHighestAltitude()
    {
        float? maxAltitude = null;
        foreach (float altitude in NetworkManager.Instance.GetActivePlayers()
                     .Select(player => player.Character)
                     .Where(character => character != null)
                     .Select(character => character.Height))
        {
            if (!maxAltitude.HasValue || altitude > maxAltitude)
            {
                maxAltitude = altitude;
            }
        }
        return maxAltitude;
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

public struct ChunksList : INetworkStruct
{
    const int MaxChunksInNetworkList = 64;
    
    public int Count => _count;
    public NetworkBehaviourId TopChunk => _count == 0 ? default : Chunks[_count - 1];
    public NetworkBehaviourId BottomChunk => _count == 0 ? default : Chunks[0];

    private int _count;

    [Networked, Capacity(MaxChunksInNetworkList)]
    public NetworkArray<NetworkBehaviourId> Chunks => default;
    

    public bool AddChunkToTop(Chunk chunk)
    {
        if (_count == MaxChunksInNetworkList)
        {
            Debug.LogError("Unable to add more chunks to NetworkArray. No space.");
            return false;
        }
        Chunks.Set(_count, chunk);
        _count++;
        return true;
    }

    public bool RemoveBottomChunk()
    {
        if (_count == 0) return false;
        for (int i = 1; i < _count; i++)
        {
            Chunks.Set(i - 1, Chunks[i]);
        }
        _count--;
        return true;
    }
}
