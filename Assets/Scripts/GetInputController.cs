using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class GetInputController : NetworkBehaviour, IPlayerActionsGetter
{
    public int HorizontalMoveDirection => _horizontalMoveDirection;
    public event Action PressJump;
    public event Action PressPushPlatform;

    private int _horizontalMoveDirection = 0;
    private bool _lastPressJumpState = false;
    private bool _lastPushPlatformState = false;

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData inputData))
        {
            _horizontalMoveDirection = inputData.Direction;
            if (!_lastPressJumpState && inputData.Jumped)
                PressJump?.Invoke();
            if (!_lastPushPlatformState && inputData.PushedPlatform)
                PressPushPlatform?.Invoke();
            _lastPressJumpState = inputData.Jumped;
            _lastPushPlatformState = inputData.PushedPlatform;
        }
    }
}
