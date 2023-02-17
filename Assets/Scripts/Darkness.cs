using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Darkness : NetworkBehaviour
{
    public static Darkness Instance { get; private set; } 
    public float Altitude => transform.position.y;
}
