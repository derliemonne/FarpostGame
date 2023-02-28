using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.TextCore.Text;

public class PlayerSound : NetworkSound
{
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _buffSound;

    private Character _character;

    protected override Dictionary<string, AudioClip> MakeSoundMap()
    {
        return new Dictionary<string, AudioClip>()
        {
            {"jump", _jumpSound},
            {"death", _deathSound},
            {"buff", _buffSound},
        };
    }

    public void BindPlayer(Character character)
    {
        _character = character;
        _character.BindPlayerSound(this);
    }

    public void Play_Death()
    {
        PlayToOne(_character.PlayerId, "death");
    }

    public void Play_Jump()
    {
        PlayToOne(_character.PlayerId, "jump");
    }

    public void Play_Buff()
    {
        PlayToOne(_character.PlayerId, "buff");
    }
}
