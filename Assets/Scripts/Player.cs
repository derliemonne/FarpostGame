using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class Player : NetworkBehaviour
{
    public event Action<CharacterType> ChosenCharacterChanged;
    
    [Networked] public PlayerRef PlayerRef { get; set; }
    [Networked] public NetworkBool IsReadyToStartRace { get; set; }
    
    /// <summary>
    /// Index in NetworkManager.Instance.CharacterPrefabs list.
    /// </summary>
    [Networked(OnChanged = nameof(OnChosenCharacterChanged))] public CharacterType ChosenCharacter { get; set; }
    [Networked] public Character Character { get; set; }

    public static void OnChosenCharacterChanged(Changed<Player> changed)
    {
        Player player = changed.Behaviour;
        player.ChosenCharacterChanged?.Invoke(player.ChosenCharacter);
    }
}
