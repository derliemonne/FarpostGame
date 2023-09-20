using UnityEngine;
using Fusion;
using System.Collections;

public class PushPlatformScript : NetworkBehaviour
{
    [SerializeField] private float _pushPlatformDist = 1f;
    [SerializeField] private float _pushPlatformCooldown = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private IPlayerActionsGetter _playerActionsGetter;

    private bool _canPushPlatform = false;

    private void Awake()
    {
        _playerActionsGetter.PressPushPlatform += () =>
        {
            PushPlatform();
            StartCoroutine(ResetPushPlatform());
        };
    }

    private void PushPlatform()
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, _pushPlatformDist, _groundLayer);
        if (raycastHit.transform != null)
        {
            Platform platformScript = raycastHit.transform.GetComponent<Platform>();
            if (platformScript != null)
            {
                platformScript.PushPlatform();
            }
        }
    }

    private IEnumerator ResetPushPlatform()
    {
        _canPushPlatform = false;
        yield return new WaitForSeconds(_pushPlatformCooldown);
        _canPushPlatform = true;
    }
}
