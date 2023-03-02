using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputButtonUp : MonoBehaviour
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite _canJumpSprite;
    [SerializeField] private Sprite _doubleJumpActiveSprite;
    [SerializeField] private Sprite _canNotJumpSprite;

    public void SetButtonImage(bool canJump, bool isDoubleJumpActive)
    {
        if (_targetImage == null)
        {
            Debug.LogError($"InputButtonUp is broken. {_targetImage} is null.");
            return;
        }
        _targetImage.sprite = isDoubleJumpActive ? 
            _doubleJumpActiveSprite :
            (canJump ? _canJumpSprite : _canNotJumpSprite);

    }
}
