using System;
using System.Collections.Generic;
using Fusion;
using JetBrains.Annotations;
using Mono.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using ReadOnlySpawnPointsCollection = System.Collections.ObjectModel.ReadOnlyCollection<SpawnPoint>;

public class Chunk : NetworkBehaviour
{
    public const int ChunkHeightPixels = 1920 * 2;
    public const int ChunkWidthPixels = 1080;
    public const float PixelsPerUnit = 100;
    public const float ChunkHeight = ChunkHeightPixels / PixelsPerUnit;
    public const float ChunkWidth = ChunkWidthPixels / PixelsPerUnit;
    public const float ChunkHalfHeight = ChunkHeight / 2;
    public const float ChunkHalfWidth = ChunkWidth / 2;
        
    public float Altitude => transform.position.y;

    [SerializeField] private List<SpawnPoint> _unsortedSpawnPoints = new List<SpawnPoint>();
    public ReadOnlySpawnPointsCollection SortedSpawnPoints { get; private set; }

    private void Awake()
    {
        SortSpawnPoints();
    }

    private void SortSpawnPoints()
    {
        List<SpawnPoint> spawnPointsList = new List<SpawnPoint>();

        foreach(SpawnPoint sp in _unsortedSpawnPoints)
        {
            if (sp != null) spawnPointsList.Add(sp);
        }

        spawnPointsList.Sort(delegate (SpawnPoint sp1, SpawnPoint sp2)
        {
            return sp1.Altitude.CompareTo(sp2.Altitude);
        });

        SortedSpawnPoints = spawnPointsList.AsReadOnly();
    }

    [Pure]
    public bool IsAltitudeFitsInChunk(float altitude)
    {
        float chunkAltitude = Altitude;
        return chunkAltitude - ChunkHalfHeight <= altitude && altitude <= chunkAltitude + ChunkHalfHeight;
    }

    [Pure]
    public Rect GetRect() => new(transform.position, new Vector2(ChunkWidth, ChunkHeight));

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(ChunkWidth, ChunkHeight, 0));
    }
}
