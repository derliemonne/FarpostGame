using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistEffect : Effect
{
    public override string EffectName => "Resist";
    public override float Duration => 5;
    public override bool IsInstant => false;

    public ResistEffect(PlayersList playersList, Character character) : base(playersList, character)
    {
        
    }

    public override void GiveEffect()
    {
        effectedPlayer.ResistSphere.SetActive(true);
    }

    public override void TakeOffEffect()
    {
        effectedPlayer.ResistSphere.SetActive(false);
    }
}
