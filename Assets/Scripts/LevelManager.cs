using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Server-only gameObject.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Chunk> _chunkPrefabs;
    [SerializeField] private float _firstChunkAltitude = Chunk.ChunkHalfHeight;
    
    /// <summary>
    /// First chunk is the bottom. Last chunk is highest.
    /// </summary>
    private readonly LinkedList<Chunk> _chunks = new();
    
    public void SpawnChunkOnTop()
    {
        LinkedListNode<Chunk> topChunk = _chunks.Last;
        float newChunkAltitude = topChunk == null ? _firstChunkAltitude : topChunk.Value.Altitude + Chunk.ChunkHeight;
        Vector3 newChunkPosition = new(0, newChunkAltitude, 0);
        Debug.Log($"SpawnChunk at altitude {newChunkPosition.y}");
        Chunk chunk = NetworkManager.Instance.NetworkRunner.Spawn(_chunkPrefabs[0], newChunkPosition);
        if (chunk == null)
        {
            Debug.LogError("Chunk prefab is invalid.");
            return;
        }
        _chunks.AddLast(chunk);
    }

    public void DespawnChunkFromBottom()
    {
        LinkedListNode<Chunk> bottomChunk = _chunks.First;
        if (bottomChunk == null) return;
        NetworkManager.Instance.NetworkRunner.Despawn(bottomChunk.Value.Object);
        _chunks.RemoveFirst();
    }
    private void FixedUpdate()
    {
        LinkedListNode<Chunk> topChunk = _chunks.Last;
        if (topChunk == null) return;
        float? charactersHighestAltitude = GetCharactersHighestAltitude();
        if (!charactersHighestAltitude.HasValue) return;
        if (topChunk.Value.IsAltitudeFitsInChunk(charactersHighestAltitude.Value))
        {
            SpawnChunkOnTop();
        }
        
        while (_chunks.Count != 0)
        {
            LinkedListNode<Chunk> bottomChunk = _chunks.First;
            float darknessAltitude = Darkness.Instance.Altitude;
            // float darknessAltitude = 0;
            bool darknessAbsorbedChunk = bottomChunk.Value.Altitude + Chunk.ChunkHeight < darknessAltitude;
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
}
