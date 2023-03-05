using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TouchButton : Button
{
    public event Action GetButtonDown;
    public event Action GetButtonUp;

    public new bool IsPressed => IsPressed();

    private bool _wasTouchingButton = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {   
        if (IsPressed && !_wasTouchingButton)
        {
            GetButtonDown?.Invoke();
        }

        if(!IsPressed && _wasTouchingButton)
        {
            GetButtonUp?.Invoke();
        }

        _wasTouchingButton = IsPressed;
    }
}
