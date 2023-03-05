using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MoveController : MonoBehaviour
{
    public int HorizontalInput { get; private set; } = 0;
    public bool IsJumping { get; private set; } = false;
    public bool IsPushingPlatform { get; private set; } = false;

    [Header("Touch zones")]
    [SerializeField] private RectTransform _leftTouchZone;
    [SerializeField] private RectTransform _rightTouchZone;

    [Header("Jump input")]
    [SerializeField] private GameObject _sliderBackgroundGameObject;
    [SerializeField] private JumpSliderButton _jumpSliderButton;
    [SerializeField] private Slider _jumpSlider;

    [Header("Push platform")]
    [SerializeField] private InputButton _pushPlatformButton;

    private void Start()
    {
        _jumpSliderButton.GetButtonDown += () =>
        {
            SetActiveSliderJumpMode(true);
            IsJumping = true;
        };

        _jumpSliderButton.GetButtonUp += () =>
        {
            SetActiveSliderJumpMode(false);
            IsJumping = false;
        };
    }

    private void Update()
    {
        bool onLeft = Input.touches.Any(touch => _leftTouchZone.rect.Contains(touch.position - (Vector2)_leftTouchZone.position));
        bool onRight = Input.touches.Any(touch => _rightTouchZone.rect.Contains(touch.position - (Vector2)_rightTouchZone.position));
        
        if(onLeft)
        {
            HorizontalInput = -1;
        }
        else if(onRight)
        {
            HorizontalInput = 1;
        }
        else
        {
            HorizontalInput = 0;
        }

        IsPushingPlatform = _pushPlatformButton.IsPressed;

        //move slider to the mid when users's finger doesnt above it
        if (!IsJumping) _jumpSlider.value = 0;

        print(this);
    }

    private void SetActiveSliderJumpMode(bool value)
    {
        _sliderBackgroundGameObject.SetActive(value);
        if(!value)
        {
            _jumpSlider.value = 0;
        }
    }

    public override string ToString()
    {
        return $"HI : {HorizontalInput} | IsJumping : {IsJumping} | IsPushingPlatform : {IsPushingPlatform}";
    }
}
