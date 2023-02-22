using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContinuousEffect: Effect
{
    public abstract float Duration { get; }

    public ContinuousEffect(EffectManager effectManager) : base(effectManager)
    {

    }

    public abstract void Remove();
}
