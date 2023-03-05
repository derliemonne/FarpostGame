using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpSliderButton : MonoBehaviour
{
    public event Action GetButtonDown;
    public event Action GetButtonUp;

    public bool IsPressed { get; private set; } = false;

    //indicates if user touched the screen anywhere at last frame
    private bool _wasTouchingScreen = false;
    private RectTransform _jumpButtonRect;

    private void Awake()
    {
        _jumpButtonRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        bool isTouchingJumpButton = Input.touches.Any(touch => _jumpButtonRect.rect.Contains(touch.position - (Vector2)_jumpButtonRect.position));
        
        if (isTouchingJumpButton && !_wasTouchingScreen)
        {
            IsPressed = true;
            GetButtonDown?.Invoke();
        }

        if(IsPressed && !isTouchingJumpButton)
        {
            IsPressed = false;
            GetButtonUp?.Invoke();
        }

        _wasTouchingScreen = Input.touchCount != 0;
    }
}
