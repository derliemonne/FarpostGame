using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBootsEffect : ContinuousEffect
{
    public override string EffectName => "Ice boots";
    public override int EffectId => 1;
    public override float Duration { get; protected set; } = 10;

    public IceBootsEffect(EffectManager effectManager, float? duration = null) : base(effectManager, duration)
    {
        
    }

    public override void Apply()
    {
        effectManager.Character.IceBoots.SetActive(true);
    }

    public override void Remove()
    {
        effectManager.Character.IceBoots.SetActive(false);
    }
}
