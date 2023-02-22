using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GullScript : Character
{
    public override CharacterType CharacterType => CharacterType.Gull;
    public override string CharacterName => "Чайка";

    [Networked] private bool _canDoubleJump { get; set; } = true;
    private float _doubleJumpCoolDown = 5f;

    protected override void Jump(float inputY, ref Vector3 velocity, bool isGrounded)
    {
        if (isGrounded)
        {
            _networkAnimator.Animator.SetBool(_isJumping, false);
        }
        else
        {
            _networkAnimator.Animator.SetBool(_isJumping, true);
        }

        if (inputY > 0.1 && _canJump)
        {
            if(isGrounded)
            {
                velocity.y = _jumpSpeed;
                ResetJump(_jumpCoolDown);
            }
            else if(_canDoubleJump)
            {
                velocity.y = _jumpSpeed;
                ResetJump(_jumpCoolDown);
                ResetDoubleJump(_doubleJumpCoolDown);
            }
        }
    }

    private void ResetDoubleJump(float duration)
    {
        _canDoubleJump = false;
        StartCoroutine(DoubleJumpDelay(duration));
    }

    private IEnumerator DoubleJumpDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        _canDoubleJump = true;
    }
}
