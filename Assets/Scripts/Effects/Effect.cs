using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public abstract string EffectName { get; }
    public abstract int EffectId { get; }

    protected EffectManager effectManager;

    public Effect(EffectManager effectManager)
    {
        this.effectManager = effectManager;
    }

    public abstract void Apply();
}
