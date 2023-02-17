using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZoneScript : NetworkBehaviour
{
    [SerializeField] private PlayersList _plList;
    private Transform _deathZoneTr;
    private SpriteRenderer _deathZoneRenderer;

    [Networked] public float Altitude { get; private set; }

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
        Move(Time.deltaTime);
        _deathZoneTr.position = new Vector3(0, Altitude, 0);
        Debug.Log(IsActive + " : " + _deathZoneRenderer.enabled + " : " + _deathZoneTr.position.y);
    }

    private void Move(float delta)
    {
        if(Runner.IsServer && IsActive)
        {
            Altitude += Speed * delta;
        }
    }
}
