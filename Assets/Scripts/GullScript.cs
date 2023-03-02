using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GullScript : Character
{
    public event Action<bool> DoubleJumpChanged;
    public override CharacterType CharacterType => CharacterType.Gull;
    public override string CharacterName => "Чайка";

    [Networked(OnChanged = nameof(OnDoubleJumpChanged))]
    public bool DoubleJump { get; private set; } = true;
    private float _doubleJumpCoolDown = 5f;

    public static void OnDoubleJumpChanged(Changed<GullScript> changed)
    {
        changed.Behaviour.DoubleJumpChanged?.Invoke(changed.Behaviour.DoubleJump);
    }
    
    protected override void Jump(bool jump, ref Vector3 velocity, bool isGrounded)
    {
        _networkAnimator.Animator.SetBool(_isJumping, !isGrounded);

        if(isGrounded)
        {
            if (jump && CanJump)
            {
                velocity.y = _jumpSpeed;
                if (Runner.IsServer)
                {
                    if (_playerSound != null)
                    {
                        Rpc_PlayJump(PlayerId);
                    }
                    else
                    {
                        Debug.LogError("_playerSound is null");
                    }
                }
                ResetJump(_jumpCoolDown);
            }
        }
        else if (jump && DoubleJump && CanJump)
        {
            velocity.y = _jumpSpeed;
            if (Runner.IsServer)
            {
                if (_playerSound != null)
                {
                    Rpc_PlayJump(PlayerId);
                }
                else
                {
                    Debug.LogError("_playerSound is null");
                }
            }
            ResetDoubleJump(_doubleJumpCoolDown);
        }
    }

    private void ResetDoubleJump(float duration)
    {
        DoubleJump = false;
        StartCoroutine(DoubleJumpDelay(duration));
    }

    private IEnumerator DoubleJumpDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        DoubleJump = true;
    }
}
