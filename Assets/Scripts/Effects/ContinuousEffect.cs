using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContinuousEffect: Effect
{
    public abstract float Duration { get; protected set; }

    public ContinuousEffect(EffectManager effectManager, float? duration = null) : base(effectManager)
    {
        if(duration.HasValue)
        {
            Duration = duration.Value;
        }
    }

    public abstract void Remove();
}
