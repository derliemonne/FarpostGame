using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadPlatform : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D _badPlatformCollider;
    [SerializeField] private Animator _badPlatformAnimator;
    [SerializeField] private NetworkObject _badPlatformNetObject;

    private static readonly float _animDuration = 0.5f;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Rpc_BreakPlatform();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void Rpc_BreakPlatform()
    {
        BreakPlatform();
    }

    private void BreakPlatform()
    {
        _badPlatformAnimator.enabled = true;
        _badPlatformCollider.enabled = false;
        if(Runner.IsServer)
        {
            StartCoroutine(DelayToDespawn());
        }
        
    }

    private IEnumerator DelayToDespawn()
    {
        yield return new WaitForSeconds(_animDuration);
        Runner.Despawn(_badPlatformNetObject);  
    }
}
