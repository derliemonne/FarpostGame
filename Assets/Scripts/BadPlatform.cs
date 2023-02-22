using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadPlatform : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D _badPlatformCollider;
    [SerializeField] private Animator _badPlatformAnimator;
    [SerializeField] private NetworkObject _badPlatformNetObject;
    [SerializeField] private float _stunDuration;

    private static readonly float _animDuration = 0.5f;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();

        if(Runner.IsServer && character != null)
        {
            BreakPlatform();
            character.Stun(_stunDuration);
        }   
    }

    private void BreakPlatform()
    {
        Rpc_BreakPlatform();
        StartCoroutine(DelayToDespawn());
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void Rpc_BreakPlatform()
    {
        _badPlatformAnimator.enabled = true;
        _badPlatformCollider.enabled = false;
    }

    private IEnumerator DelayToDespawn()
    {
        yield return new WaitForSeconds(_animDuration);
        Runner.Despawn(_badPlatformNetObject);  
    }
}
