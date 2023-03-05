using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MoveController : MonoBehaviour
{
    public int HorizontalInput { get; private set; } = 0;

    private bool _jumpState = false;
    private bool _pushPlatformState = false;

    [Header("Touch zones")]
    [SerializeField] private RectTransform _leftTouchZone;
    [SerializeField] private RectTransform _rightTouchZone;

    [Header("Jump input")]
    [SerializeField] private GameObject _sliderBackgroundGameObject;
    [SerializeField] private TouchButton _jumpSliderButton;
    [SerializeField] private Slider _jumpSlider;

    [Header("Push platform")]
    [SerializeField] private TouchButton _pushPlatformButton;

    private void Start()
    {
        _jumpSliderButton.GetButtonDown += () =>
        {
            SetActiveSliderJumpMode(true);
            _jumpState = true;
        };

        _jumpSliderButton.GetButtonUp += () =>
        {
            SetActiveSliderJumpMode(false);
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

        //move slider to the mid when users's finger doesnt above it
        if (!_jumpSliderButton.IsPressed) _jumpSlider.value = 0;

        print(this);
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

    private void SetActiveSliderJumpMode(bool value)
    {
        _sliderBackgroundGameObject.SetActive(value);
        if(!value)
        {
            _jumpSlider.value = 0;
        }
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
