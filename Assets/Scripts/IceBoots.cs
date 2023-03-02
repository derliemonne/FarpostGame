using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBoots : NetworkBehaviour
{
    public bool IsActive { get; private set; }

    [SerializeField] private SpriteRenderer _iceBootsRenderer;
    [SerializeField] private BoxCollider2D _iceBootsCollider;
    [SerializeField] private Animator _iceBootsAnimator;

    public void SetActive(bool value)
    {
        Rpc_SetActiveForAll(value);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsActive && Runner.IsServer)
        {
            Platform platform = collision.GetComponent<Platform>();
            if(platform != null)
            {
                platform.FrozePlatform();
            }
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void Rpc_SetActiveForAll(bool value)
    {
        _iceBootsRenderer.enabled = value;
        _iceBootsCollider.enabled = value;
        _iceBootsAnimator.enabled = value;
        IsActive = value;
    }
}
