using UnityEngine;
using Fusion;
using System;
using System.Collections;

//tip: logic of simple movement carry to another class like MoveScript

/// <summary>
/// Class that encapsulate character movement logic
/// </summary>
public class CharacterMoveScript : MoveScript
{
    [SerializeField] protected IPlayerActionsGetter _playerActionsGetter;
    [SerializeField] protected IGroundChecker _groundChecker;

    //tip: replace to another interface
    public float MoveSpeed { get; set; }
    public float JumpSpeed { get; set; }
    public float MoveInAirSpeed { get; set; }
    public float AccelerationCoef { get; set; }
    public float JumpDelay { get; set; }
    public bool EnableMove { get; set; } = true;
    public bool EnableJump { get; set; } = true;

    private float _interHorInput = 0;
    private bool _canJump = true;

    private void Start()
    {
        _playerActionsGetter.PressJump += JumpController;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        JumpController();
    }

    /// <summary>
    /// Controll for the jumping of player and reseting jump. Overrided in GullScript
    /// </summary>
    protected virtual void JumpController()
    {
        if (_groundChecker.OnGround && _canJump && EnableJump)
        {
            Jump();
            StartCoroutine(ResetJump());
        }
    }

    protected override void Move()
    {
        if(EnableMove) _interHorInput += _playerActionsGetter.HorizontalMoveDirection * AccelerationCoef;
        _interHorInput -= 0.1f * AccelerationCoef * _interHorInput;
        _interHorInput = Math.Min(1, _interHorInput);
        _interHorInput = Math.Max(-1, _interHorInput);

        Vector2 velocity = _movableRigidbody.velocity;
        velocity.x += _interHorInput * (_groundChecker.OnGround ? MoveSpeed : MoveInAirSpeed);
        _movableRigidbody.velocity = velocity;
    }

    protected void Jump()
    {
        Vector2 velocity = _movableRigidbody.velocity;
        velocity.y = JumpSpeed;
        _movableRigidbody.velocity = velocity;
    }

    private IEnumerator ResetJump()
    {
        _canJump = false;
        yield return new WaitForSeconds(JumpDelay);
        _canJump = true;
    }
}
