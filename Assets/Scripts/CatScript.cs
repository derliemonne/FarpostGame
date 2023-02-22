using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatScript : Character
{
    public override CharacterType CharacterType => CharacterType.Pirsik;
    public override string CharacterName => "Пырсик";

    [SerializeField] private LayerMask _crateTopLayerMask;
    
    private IEnumerator DodgeStunAnim(float animSpeed, Color oldColor, float toV, Vector3 oldScale, float toScale)
    {
        float elapsedTime = 0;
        Color newColor = Color.HSVToRGB(0, 0, toV);
        Vector3 newScale = new Vector3(toScale, toScale, 1);
        while (elapsedTime < animSpeed)
        {
            _spriteRenderer.color = Color.Lerp(oldColor, newColor, elapsedTime / animSpeed);
            _spriteTransform.localScale = Vector3.Lerp(oldScale, newScale, elapsedTime / animSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _spriteRenderer.color = newColor;
        _spriteTransform.localScale = newScale;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void Rpc_StartDodgeStun()
    {
        StartCoroutine(DodgeStunAnim(0.3f, _spriteRenderer.color, 0.6f, _spriteTransform.localScale, 0.8f));
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void Rpc_EndDodgeStun()
    {
        StartCoroutine(DodgeStunAnim(0.3f, _spriteRenderer.color, 1f, _spriteTransform.localScale, 1f));
    }

    private bool CheckFootsOnCrateTop()
    {
        return _groundChecker.IsTouchingLayers(_crateTopLayerMask);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (CheckFootsOnCrateTop()) print(CheckFootsOnCrateTop());
        if (Runner != null && Runner.IsServer &&
            collision.TryGetComponent(out Crate crate) && !CheckFootsOnCrateTop())
        {
            Rpc_StartDodgeStun();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Runner != null && Runner.IsServer && collision.TryGetComponent(out Crate crate))
        {
            Rpc_EndDodgeStun();
        }
    }
}
