using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonScript : Character
{
    public override CharacterType CharacterType => CharacterType.Marsik;
    public override string CharacterName => "Марсик";

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (Runner.IsServer)
        {
            Crate crate = collision.GetComponent<Crate>();
            Character character = collision.GetComponent<Character>();

            if (crate != null && !_resistSphere.IsActive)
            {
                Stun(5);
            }

            if(character != null && !character.ResistSphere.IsActive)
            {
                character.Stun(2);
            }
        }
    }
}
