using UnityEngine;

public class TpEffect : InstantEffect
{
    private Transform tpedTransform;
    private Vector3 tpPoint;

    public override string EffectName => "Tp";
    public override int EffectId => 0;

    public TpEffect(EffectManager effectManager, Transform tpedTransform, Vector3 tpPoint) : base(effectManager)
    {
        this.tpedTransform = tpedTransform;
        this.tpPoint = tpPoint;
    }

    public override void Apply()
    {
        this.tpedTransform.position = this.tpPoint;
    }
}
