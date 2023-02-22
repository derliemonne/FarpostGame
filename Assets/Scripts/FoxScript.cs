using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxScript : Character
{
    public override CharacterType CharacterType => CharacterType.Firsik;
    public override string CharacterName => "Фырсик";

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (Runner.IsServer)
        {
            Crate crate = collision.GetComponent<Crate>();

            if (crate != null && !_resistSphere.IsActive)
            {
                Stun(5);
            }
        }
    }
}
