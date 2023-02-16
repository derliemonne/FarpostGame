using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public virtual string EffectName
    {
        get { return "none"; }
    }

    public virtual bool IsInstant
    {
        get { return false; }
    }

    public virtual float Duration
    {
        get { return -1; }
    }

    protected PlayersList playersList;
    protected Character effectedPlayer;
    
    public Effect(PlayersList playersList, Character effectedPlayer)
    {
        this.playersList = playersList;
        this.effectedPlayer = effectedPlayer;
    }

    public virtual void GiveEffect()
    {

    }

    public virtual void TakeOffEffect()
    {

    }
}
