using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAssets : MonoBehaviour
{
    [SerializeField] private GameObject _resistSphere;
    [SerializeField] private GameObject _iceBoots;

    public static EffectAssets Instance { get; private set; }
    public GameObject ResistSphere => _resistSphere;
    public GameObject IceBoots => _iceBoots;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }
}
