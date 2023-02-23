using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonOpenMessageBox : MonoBehaviour
{
    [SerializeField] private MessageBox _messageBox;
    private Button _button;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (_messageBox == null)
        {
            Debug.LogWarning("MessageBox is null.");    
            return;
        }
        _messageBox.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_button == null) return;
        _button.onClick.RemoveAllListeners();
    }
}
