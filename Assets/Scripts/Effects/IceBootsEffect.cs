using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBootsEffect : ContinuousEffect
{
    public override string EffectName => "Ice boots";
    public override int EffectId => 1;
    public override float Duration { get; protected set; } = 10;

    private GameObject _iceBootsGameObject;

    public IceBootsEffect(EffectManager effectManager, float? duration = null) : base(effectManager, duration)
    {
        
    }

    public override void Apply()
    {
        _iceBootsGameObject = Object.Instantiate(effectAssets.IceBoots, effectManager.Character.PlayersLegsTransform.position, new Quaternion());
    }

    public override void Remove()
    {
        if(_iceBootsGameObject != null)
        {
            Object.Destroy(_iceBootsGameObject);
        }
        else
        {
            Debug.LogError("At the moment of removing IceBoots effect there were no ice boots");
        }    
    }
}
