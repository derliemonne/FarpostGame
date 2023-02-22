using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class EffectsGenerator : MonoBehaviour
{
    public static EffectsGenerator Instance { get; private set; }

    [SerializeField] private int _countOfEffects = 3;
    public int CountOfEffects => _countOfEffects;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public Effect TakeRandomEffect(EffectManager effectManager)
    {
        int effectNum = Random.Range(0, _countOfEffects);
        Debug.Log(effectNum);

        switch (effectNum)
        {
            case 0:
                Player randomPlayer = NetworkManager.Instance.GetRandomActivePlayerWithException(effectManager.Character.PlayerId);
                if (randomPlayer == null)
                {
                    Debug.LogError("One player only, tp to himself");
                    return new TpEffect(effectManager, effectManager.Character, effectManager.Character);
                }
                else
                {
                    return new TpEffect(effectManager, effectManager.Character, randomPlayer.Character);
                }

            case 1:
                return new IceBootsEffect(effectManager);

            case 2:
                return new ResistEffect(effectManager);

            default:
                Debug.LogError("Unknown effect id");
                return null;
        }
    }
}
