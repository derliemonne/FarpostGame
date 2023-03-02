using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : NetworkBehaviour
{
    [SerializeField] public float SkiCoefficient = 1f;

    [SerializeField] private Sprite _idlePlatform;
    [SerializeField] private Animator _platformAnim;
    [SerializeField] private SpriteRenderer _platformRenderer;
    [SerializeField] private BoxCollider2D _platformCollider;
    [SerializeField] private Sprite _frozenPlatformSprite;

    private bool _isFrozen = false;
    public bool IsFrozen => _isFrozen;

    private static readonly float _animDuration = 2;

    public void PushPlatform()
    {
        Rpc_PushPlatform();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void Rpc_PushPlatform()
    {
        _pushPlatform();
    }

    private IEnumerator DelayToPushBack()
    {
        yield return new WaitForSeconds(_animDuration);
        _platformAnim.enabled = false;
        _platformCollider.enabled = true;
        if(_isFrozen)
        {
            _platformRenderer.sprite = _frozenPlatformSprite;
        }
        else
        {
            _platformRenderer.sprite = _idlePlatform;
        }    
    }

    private void _pushPlatform()
    {
        _platformAnim.enabled = true;
        //_platformCollider.enabled = false;
        StartCoroutine(DelayToPushBack());
    }

    public void FrozePlatform()
    {
        Rpc_FrozePlatform();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void Rpc_FrozePlatform()
    {
        _frozePlatform();
    }

    private void _frozePlatform()
    {
        _isFrozen = true;
        _platformRenderer.sprite = _frozenPlatformSprite;
    }

    //сделать потом размораживание платформы
}
