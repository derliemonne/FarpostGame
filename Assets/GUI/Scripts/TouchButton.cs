using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TouchButton : MonoBehaviour
{
    public event Action GetButtonDown;
    public event Action GetButtonUp;

    public bool IsPressed { get; private set; } = false;

    //indicates if user touched the screen anywhere at last frame
    private bool _wasTouchingScreen = false;
    private RectTransform _touchButtonRect;

    private void Awake()
    {
        _touchButtonRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        bool isTouchingButton = Input.touches.Any(touch => _touchButtonRect.rect.Contains(touch.position - (Vector2)_touchButtonRect.position));
        
        if (isTouchingButton && !_wasTouchingScreen)
        {
            IsPressed = true;
            GetButtonDown?.Invoke();
        }

        if(IsPressed && !isTouchingButton)
        {
            IsPressed = false;
            GetButtonUp?.Invoke();
        }

        _wasTouchingScreen = Input.touchCount != 0;
    }
}
