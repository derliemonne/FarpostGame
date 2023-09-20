using UnityEngine;

public class StunEffect : ContinuousEffect
{
    private Character stunningCharacter;

    public override string EffectName => "Stun";
    public override int EffectId => 0;
    public override float Duration { get; protected set; } = 2;

    public StunEffect(EffectManager effectManager, Character stunningCharacter, float? duration = null) : base(effectManager, duration)
    {
        this.stunningCharacter = stunningCharacter;
    }

    public override void Apply()
    {
        stunningCharacter.CharacterMoveScript.EnableMove = false;
        stunningCharacter.CharacterMoveScript.EnableJump = false;
    }

    public override void Remove()
    {
        stunningCharacter.CharacterMoveScript.EnableMove = true;
        stunningCharacter.CharacterMoveScript.EnableJump = true;
    }
}
