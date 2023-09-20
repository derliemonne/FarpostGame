using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public abstract string EffectName { get; }
    public abstract int EffectId { get; }

    protected EffectManager effectManager;
    protected EffectAssets effectAssets;

    public Effect(EffectManager effectManager)
    {
        this.effectManager = effectManager;
        this.effectAssets = EffectAssets.Instance;
    }

    public abstract void Apply();
}
