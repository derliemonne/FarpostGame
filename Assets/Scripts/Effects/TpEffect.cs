using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpEffect : Effect
{
    private Transform anotherTp;

    public override string EffectName => "Tp";
    public override float Duration => -1;
    public override bool IsInstant => true;

    public TpEffect(PlayersList playersList, Character character, Transform anotherTp) : base(playersList, character)
    {
        this.anotherTp = anotherTp;
    }

    public override void GiveEffect()
    {
        effectedPlayer.transform.position = anotherTp.position;
    }
}
