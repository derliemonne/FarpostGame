using UnityEngine;

public class SwapEffect : InstantEffect
{
    private Character first;
    private Character second;

    public override string EffectName => "Tp";
    public override int EffectId => 0;

    public SwapEffect(EffectManager effectManager, Character first, Character second) : base(effectManager)
    {
        this.first = first;
        this.second = second;
    }

    public override void Apply()
    {
        Vector3 tpPosition = first.transform.position;
        first.transform.position = second.transform.position;
        second.transform.position = tpPosition;
    }
}
