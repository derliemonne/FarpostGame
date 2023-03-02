using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistSphere : NetworkBehaviour
{
    public bool IsActive { get; private set; }

    [SerializeField] private SpriteRenderer _resistSphereRenderer;
    [SerializeField] private Animator _resistSphereAnimator;

    public void SetActive(bool value)
    {
        Rpc_SetActiveForAll(value);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void Rpc_SetActiveForAll(bool value)
    {
        _resistSphereRenderer.enabled = value;
        _resistSphereAnimator.enabled = value;
        IsActive = value;
    }
}
