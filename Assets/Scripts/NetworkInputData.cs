using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 Direction;
    public bool Jumped;
    public bool PushedPlatform;
}
