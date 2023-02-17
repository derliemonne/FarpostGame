using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneScript : NetworkBehaviour
{
    [SerializeField] private PlayersList _plList;
    private Transform _deathZoneTr;
    private SpriteRenderer _deathZoneRenderer;

    [Networked] public float Altitude { get; private set; } = -6;

    public float Speed;
    public bool IsActive { get; private set; } 

    public void SetActive(bool value)
    {
        IsActive = value;
        Rpc_SetActivateDeathZone(value);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_SetActivateDeathZone(bool value)
    {
        _deathZoneRenderer.enabled = value;
    }

    public void Start()
    {
        _deathZoneTr = gameObject.GetComponent<Transform>();
        _deathZoneRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public override void FixedUpdateNetwork()
    {
        Move();
        _deathZoneTr.position = new Vector3(0, Altitude, 0);
        Debug.Log(IsActive + " : " + _deathZoneRenderer.enabled + " : " + _deathZoneTr.position.y);
    }

    private void Move()
    {
        if(Runner.IsServer && IsActive)
        {
            Altitude += Speed * Time.deltaTime;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsActive)
        {
            switch (collision.tag)
            {
                case "Fox":
                    _plList.Fox.TakeDamage();
                    break;

                case "Cat":
                    _plList.Cat.TakeDamage();
                    break;

                case "Raccoon":
                    _plList.Raccoon.TakeDamage();
                    break;

                case "Gull":
                    _plList.Gull.TakeDamage();
                    break;
            }
        }
    }
}
