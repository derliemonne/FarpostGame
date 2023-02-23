using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public NetworkBool IsReadyToStartRace { get; set; }
    
    /// <summary>
    /// Index in NetworkManager.Instance.CharacterPrefabs list.
    /// </summary>
    [Networked] public CharacterType ChosenCharacter { get; set; }
    [Networked] public Character Character { get; set; }
}
