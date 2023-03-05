using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSound : NetworkBehaviour
{
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _buffSound;

    private Character _character;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void BindPlayer(Character character)
    {
        _character = character;
        _character.BindPlayerSound(this);
    }

    public void Play_Death()
    {
        Play(_deathSound);
    }

    public void Play_Jump()
    {
        Play(_jumpSound);
    }

    public void Play_Buff()
    {
        Play(_buffSound);
    }

    private void Play(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
