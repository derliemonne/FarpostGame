using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MoveController : MonoBehaviour
{
    public int HorizontalInput { get; private set; } = 0;

    [Header("Touch zones")]
    [SerializeField] private RectTransform _leftTouchZone;
    [SerializeField] private RectTransform _rightTouchZone;

    [Header("Jump input")]
    [SerializeField] private TouchButton _jumpButton;
    [SerializeField] private RectTransform _jumpButtonRect;
    [SerializeField] private RectTransform _jumpButtonStartPosition;

    [Header("Push platform")]
    [SerializeField] private TouchButton _pushPlatformButton;

    private bool _jumpState = false;
    private bool _pushPlatformState = false;

    private void Start()
    {
        _jumpButton.GetButtonDown += () =>
        {
            _jumpState = true;
        };

        _jumpButton.GetButtonUp += () =>
        {
            _jumpButtonRect.position = _jumpButtonStartPosition.position;
        };

        _pushPlatformButton.GetButtonDown += () =>
        {
            _pushPlatformState = true;
        };
    }

    private void Update()
    {
        bool onLeft = Input.touches.Any(touch => _leftTouchZone.rect.Contains(touch.position - (Vector2)_leftTouchZone.position));
        bool onRight = Input.touches.Any(touch => _rightTouchZone.rect.Contains(touch.position - (Vector2)_rightTouchZone.position));

        if(_jumpButton.IsPressed)
        {
            var touches = Input.touches
                .Where(touch => _jumpButtonRect.rect.Contains(touch.position - (Vector2)_jumpButtonRect.position));
            if (touches.Any())
            {
                _jumpButtonRect.position = touches.First().position;
            }
        }

        HorizontalInput = 0;
        if (onLeft)
        {
            HorizontalInput = -1;
        }
        else if (onRight)
        {
            HorizontalInput = 1;
        }
    }

    public override string ToString()
    {
        return $"HI : {HorizontalInput} | _jumpState : {_jumpState} | _pushPlatformState : {_pushPlatformState}";
    }

    public bool ReadJumpState()
    {
        return ReadState(ref _jumpState);
    }

    public bool ReadPushPlatformState()
    {
        return ReadState(ref _pushPlatformState);
    }

    private bool ReadState(ref bool state)
    {
        if (state)
        {
            state = false;
            return true;
        }
        else
        {
            return false;
        }
    }
}
