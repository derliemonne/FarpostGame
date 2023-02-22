using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BuffScript : NetworkBehaviour
{
    [SerializeField] private NetworkObject buffGm;

    public Effect GetBuff(EffectManager effectManager)
    {
        Runner.Despawn(buffGm);
        return EffectsGenerator.Instance.TakeRandomEffect(effectManager);
    }
}
