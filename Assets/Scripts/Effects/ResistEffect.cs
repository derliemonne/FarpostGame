using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistEffect : ContinuousEffect
{
    public override string EffectName => "Resist";
    public override int EffectId => 2;
    public override float Duration { get; protected set; } = 10;

    public ResistEffect(EffectManager effectManager, float? duration = null) : base(effectManager, duration)
    {

    }

    public override void Apply()
    {
        effectManager.Character.ResistSphere.SetActive(true);
    }

    public override void Remove()
    {
        effectManager.Character.ResistSphere.SetActive(false);
    }
}
