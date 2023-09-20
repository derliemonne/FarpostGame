using Fusion;
using System.Linq;
using UnityEngine;

public class TouchInputController : NetworkBehaviour, IPlayerActionsSetter
{
    [Header("Touch zones")]
    [SerializeField] private RectTransform _leftTouchZone;
    [SerializeField] private RectTransform _rightTouchZone;

    [Header("Jump input")]
    [SerializeField] private TouchButton _jumpButton;
    [SerializeField] private RectTransform _jumpButtonRect;
    [SerializeField] private RectTransform _jumpButtonStartPosition;

    [Header("Push platform")]
    [SerializeField] private TouchButton _pushPlatformButton;

    public int HorizontalMoveDirection => _horizontalMoveDirection;
    public bool IsPressingJump => _jumpButton.IsPressed;
    public bool IsPressingPushPlatform => _pushPlatformButton.IsPressed;

    private int _horizontalMoveDirection = 0;

    private void Start()
    {
        _jumpButton.GetButtonUp += () =>
        {
            _jumpButtonRect.position = _jumpButtonStartPosition.position;
        };
    }

    private void Update()
    {
        bool onLeft = Input.touches.Any(touch => _leftTouchZone.rect.Contains(touch.position - (Vector2)_leftTouchZone.position));
        bool onRight = Input.touches.Any(touch => _rightTouchZone.rect.Contains(touch.position - (Vector2)_rightTouchZone.position));

        _horizontalMoveDirection = onRight ? 1 : onLeft ? -1 : 0;
    }
}