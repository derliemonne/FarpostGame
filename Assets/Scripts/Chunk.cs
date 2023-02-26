using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fusion;
using JetBrains.Annotations;
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

    private static bool _notFirstTeleport = false;
    private static Teleport _lastTeleport;
        
    public float Altitude => transform.position.y;

    public ReadOnlySpawnPointsCollection SortedSpawnPoints { get; private set; }
    public ReadOnlyCollection<Teleport> SortedTeleports { get; private set; }

    [SerializeField] private List<SpawnPoint> _unsortedSpawnPoints = new List<SpawnPoint>();
    [SerializeField] private List<Teleport> _unsortedTeleports = new List<Teleport>();

    private void Awake()
    {
        SortSpawnPoints();
        SortTeleports();  
    }

    private void Start()
    {      
        BindTeleports();
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

    private void SortTeleports()
    {
        List<Teleport> teleportList = new List<Teleport>();

        foreach (Teleport teleport in _unsortedTeleports)
        {
            if (teleport != null) teleportList.Add(teleport);
        }

        teleportList.Sort(delegate (Teleport tp1, Teleport tp2)
        {
            return tp1.Altitude.CompareTo(tp2.Altitude);
        });

        SortedTeleports = teleportList.AsReadOnly();
    }

    private void BindTeleports()
    {
        int countOfTeleports = SortedTeleports.Count;
        if (countOfTeleports > 0) 
        {
            if(_notFirstTeleport)
            {
                SortedTeleports[0].SetNextTp(_lastTeleport);
            }
            else
            {
                _notFirstTeleport = true;
            }
            
            for(int i = 1; i < countOfTeleports; i++)
            {
                SortedTeleports[i].SetNextTp(SortedTeleports[i - 1]);
            }

            _lastTeleport = SortedTeleports[countOfTeleports - 1];
        }
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
