using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonScript : Character
{
    public override CharacterType CharacterType => CharacterType.Marsik;
    public override string CharacterName => "Марсик";

    [SerializeField] private Collider2DEventSender eventSender;
    [SerializeField] private float _raccoonStunDuration = 2f;

    private void Start()
    {
        eventSender.TriggerEnter2D += RaccoonStun;
    }

    private void RaccoonStun(Collider2D collider)
    {
        if(Runner.IsServer && collider.gameObject.TryGetComponent(out Character character) && !character.ResistSphere.IsActive)
        {
            if(!character.ResistSphere.IsActive) character.Stun(_raccoonStunDuration);
        }
    }
}
