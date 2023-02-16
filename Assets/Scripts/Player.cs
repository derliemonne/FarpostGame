using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public NetworkBool IsReadyToStartRace { get; set; }
}
