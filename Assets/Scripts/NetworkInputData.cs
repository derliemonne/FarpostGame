using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public int Direction;
    public bool Jumped;
    public bool PushedPlatform;
}
