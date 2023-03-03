using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PushPlatformButton : MonoBehaviour
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite _activeButton;
    [SerializeField] private Sprite _inactiveButton;

    public void SetButtonImage(bool canPushPlatform)
    {
        if (_targetImage == null)
        {
            Debug.LogError($"PushPlatformButton is broken. {_targetImage} is null.");
            return;
        }
        _targetImage.sprite = canPushPlatform ? _activeButton : _inactiveButton;
    }
}
