using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistEffect : ContinuousEffect
{
    public override string EffectName => "Resist";
    public override int EffectId => 2;
    public override float Duration { get; protected set; } = 10;

    private GameObject _resistSphereGameObject;

    public ResistEffect(EffectManager effectManager, float? duration = null) : base(effectManager, duration)
    {

    }

    public override void Apply()
    {
        _resistSphereGameObject = Object.Instantiate(effectAssets.ResistSphere, effectManager.Character.PlayersLegsTransform.position, new Quaternion());
    }

    public override void Remove()
    {
        if (_resistSphereGameObject != null)
        {
            Object.Destroy(_resistSphereGameObject);
        }
        else
        {
            Debug.LogError("At the moment of removing ResistSphere effect there were no resist sphere");
        }
    }
}
