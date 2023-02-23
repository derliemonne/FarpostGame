using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public event Action Closed;

    [SerializeField] private Button _closeButton;
    [SerializeField] private TMP_Text _text;

    private void Awake()
    {
        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    private void OnCloseButtonClicked()
    {
        Closed?.Invoke();
        SetActive(false);
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
    }
}
