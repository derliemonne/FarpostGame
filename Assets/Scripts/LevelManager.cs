using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    private const int MaxChunksInNetworkList = 16;
    
    public static LevelManager Instance { get; private set; }
    
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
        Chunk chunk = NetworkManager.Instance.NetworkRunner.Spawn(_chunkPrefabs[0], newChunkPosition);
        if (chunk == null)
        {
            Debug.LogError("Chunk prefab is invalid.");
            return;
        }
        Chunks.AddChunkToTop(chunk);
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

    private Chunk GetChunk(NetworkBehaviourId networkBehaviourId)
    {
        if (networkBehaviourId == default) return null;
        return Runner.TryFindBehaviour(networkBehaviourId, out Chunk chunk) ? chunk : null;
    }
    
    private void FixedUpdate()
    {
        if (!Runner.IsServer) return;
        if (Chunks.Count == 0) return;
        Chunk topChunk = GetChunk(Chunks.TopChunk);
        if (topChunk == null) return;
        float? charactersHighestAltitude = GetCharactersHighestAltitude();
        if (!charactersHighestAltitude.HasValue) return;
        if (topChunk.IsAltitudeFitsInChunk(charactersHighestAltitude.Value))
        {
            SpawnChunkOnTop();
        }
        
        while (Chunks.Count != 0)
        {
            Chunk bottomChunk = GetChunk(Chunks.BottomChunk);
            if (bottomChunk == null) return;
            float darknessAltitude = Darkness.Instance.Altitude;
            bool darknessAbsorbedChunk = bottomChunk.Altitude + Chunk.ChunkHeight < darknessAltitude;
            if (darknessAbsorbedChunk)
            {
                DespawnChunkFromBottom();
            }
            else
            {
                break;
            }
        }
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
