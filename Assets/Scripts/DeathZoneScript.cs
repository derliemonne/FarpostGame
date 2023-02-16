using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneScript : MonoBehaviour
{
    [SerializeField] private PlayersList _plList;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _deathZoneTr;

    public float speed;

    public bool IsActive { get; private set; } 

    public void Change(bool value)
    {
        if(value)
        {
            _spriteRenderer.enabled = true;
            IsActive = true;
        }
        else
        {
            _spriteRenderer.enabled = false;
            IsActive = false;
        }
    }

    public void Move()
    {
        _deathZoneTr.Translate(new Vector3(0, speed * Time.deltaTime, 0));
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsActive)
        {
            switch (collision.tag)
            {
                case "Fox":
                    _plList.fox.TakeDamage();
                    break;

                case "Cat":
                    _plList.cat.TakeDamage();
                    break;

                case "Raccoon":
                    _plList.raccoon.TakeDamage();
                    break;

                case "Gull":
                    _plList.gull.TakeDamage();
                    break;
            }
        }
    }
}
