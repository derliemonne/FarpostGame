using System;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

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
