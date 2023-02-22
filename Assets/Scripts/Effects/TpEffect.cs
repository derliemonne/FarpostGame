using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpEffect : InstantEffect
{
    private Character first;
    private Character second;

    public override string EffectName => "Tp";
    public override int EffectId => 0;

    public TpEffect(EffectManager effectManager, Character first, Character second) : base(effectManager)
    {
        this.first = first; 
        this.second = second;
    }

    public override void Apply()
    {
        Vector3 firstPosition = first.transform.position;
        first.transform.position = second.transform.position;
        second.transform.position = firstPosition;
    }
}
