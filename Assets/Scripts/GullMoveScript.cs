using System.Collections;
using UnityEngine;

public class GullMoveScript : CharacterMoveScript
{
    public float DoubleJumpDelay { get; set; }

    private bool _canDoubleJump = true;

    protected override void JumpController()
    {
        base.JumpController();
        if(!_groundChecker.OnGround && _canDoubleJump && EnableJump)
        {
            Jump();
            StartCoroutine(ResetDoubleJump());
        }
    }

    private IEnumerator ResetDoubleJump()
    {
        _canDoubleJump = false;
        yield return new WaitForSeconds(JumpDelay);
        _canDoubleJump = true;
    }
}
