using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffScript : MonoBehaviour
{
    [SerializeField]
    private PlayersList pl_list;

    [SerializeField]
    private GameObject buff_gm;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Fox":
                pl_list.fox.TakeEffect(GetRandomEffect(pl_list.fox));
                break;

            case "Cat":
                pl_list.cat.TakeEffect(GetRandomEffect(pl_list.cat));
                break;

            case "Raccoon":
                pl_list.raccoon.TakeEffect(GetRandomEffect(pl_list.raccoon));
                break;

            case "Gull":
                pl_list.gull.TakeEffect(GetRandomEffect(pl_list.gull));
                break;
        }
        Destroy(buff_gm);
    }

    public Effect GetRandomEffect(Character player)
    {
        return new ResistEffect(pl_list, player);
    }
}
