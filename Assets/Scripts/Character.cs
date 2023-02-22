using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public enum CharacterType
{
    None,
    Firsik,
    Pirsik,
    Marsik,
    Gull
}

[RequireComponent(typeof(NetworkRigidbody2D))]
public abstract class Character : NetworkBehaviour
{
    private const float HorizontalSpeedConsideredNotMoving = 0.1f;

    /// <summary>
    /// Need override in each subclass.
    /// </summary>
    public abstract CharacterType CharacterType { get; }
    public abstract string CharacterName { get; }
    public int Health { get; private set; } = 1;
    public bool IsDead { get; private set; }
    public bool IsStunned { get; private set; }

    public float Height => transform.position.y;
    public ResistSphere ResistSphere => _resistSphere;
    public GameObject PlayerGameObject => _playerGameObject;
    public IceBoots IceBoots => _iceBoots;

    //id игрока, который владеет персонажем, в будущем можно заменить на объект класса Player
    [Networked] public int PlayerId { get; private set; } = -1;

    private static readonly int _isJumping = Animator.StringToHash("is_jumping");
    private static readonly int _isRunning = Animator.StringToHash("is_moving");
    private static readonly int _isStunnedAnim = Animator.StringToHash("is_stunned");

    [SerializeField] protected float _moveSpeed = 1f;
    [SerializeField] protected float _jumpSpeed = 5f;
    [SerializeField] protected float _pushPlatformDist = 1f;
    [SerializeField] protected Vector2 _groundCheckerSize;
    [SerializeField] protected float _groundCheckerDist;
    [SerializeField] protected LayerMask _groundLayer;
    [SerializeField] protected Transform _groundChecker;
    [SerializeField] protected NetworkMecanimAnimator _networkAnimator;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected GameObject _playerGameObject;
    [SerializeField] protected Transform _spriteTransform;
    [SerializeField] protected Transform _cameraTransform;
    [SerializeField] protected EffectManager _effectManager;
    [SerializeField] protected IceBoots _iceBoots;
    [SerializeField] protected ResistSphere _resistSphere;

    private NetworkRigidbody2D _networkRb;
    private float inputInter = 0;

    private void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody2D>();
    }

    public void SetPlayerId(int playerId)
    {
        if(PlayerId == -1)
        {
            PlayerId = playerId;
        }
    }

    public void TakeDamage()
    {
        Health--;
        if(Health <= 0)
        {
            Death();
        }
    }

    public void Stun(float duration)
    {
        Rpc_SetActiveStun(true);
        StartCoroutine(StunDuration(duration));
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    private void Rpc_SetActiveStun(bool value)
    {
        IsStunned = value;
        _networkAnimator.Animator.SetBool(_isStunnedAnim, value);
    }

    private IEnumerator StunDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Rpc_SetActiveStun(false);
    }

    public override void FixedUpdateNetwork()
    {
        //переписать стан
        if (GetInput(out NetworkInputData inputData))
        {
            RaycastHit2D raycastInfo = GroundCheck();
            Platform standOnPlatform = null;

            if (raycastInfo.transform != null)
            {
                standOnPlatform = raycastInfo.transform.GetComponent<Platform>();
            }

            Vector2 input = inputData.Direction.normalized;
            Vector2 velocity = _networkRb.Rigidbody.velocity;

            if(IsStunned)
            {
                input = Vector3.zero;
            }

            if(inputData.PushedPlatform)
            {
                RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, _pushPlatformDist, _groundLayer);
                if(raycastHit.transform != null)
                {
                    Platform platformScript = raycastHit.transform.GetComponent<Platform>();
                    if(platformScript != null)
                    {
                        platformScript.PushPlatform();
                    }
                }                            
            }

            if(raycastInfo)
            {
                if(input.y > 0.1)
                {
                    velocity.y = _jumpSpeed;
                }
                _networkAnimator.Animator.SetBool(_isJumping, false);
            }
            else
            {
                _networkAnimator.Animator.SetBool(_isJumping, true);
            }

            _networkAnimator.Animator.SetBool(_isRunning, Math.Abs(input.x) > HorizontalSpeedConsideredNotMoving);

            if(standOnPlatform != null && standOnPlatform.IsFrozen && !IceBoots.IsActive)
            {
                inputInter += input.x * standOnPlatform.SkiCoefficient;
                inputInter -= 0.1f * standOnPlatform.SkiCoefficient * Math.Sign(inputInter);
                inputInter = Math.Min(1, inputInter);
                inputInter = Math.Max(-1, inputInter);
            }
            else
            {
                inputInter = input.x;
            }

            velocity.x = inputInter * _moveSpeed;

            _networkRb.Rigidbody.velocity = velocity;
        }
    }

    private void Update()
    {
        if (_networkAnimator.Animator.GetBool(_isRunning) && 
            Math.Abs(_networkRb.Rigidbody.velocity.x) > 0.01)
        {
            _spriteRenderer.flipX = _networkRb.Rigidbody.velocity.x < 0;
        }
    }

    private void Death()
    {
        gameObject.SetActive(false);
        IsDead = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(
            new Vector3(_groundChecker.position.x, _groundChecker.position.y - _groundCheckerDist / 2), 
            new Vector3(_groundCheckerSize.x, _groundCheckerSize.y + _groundCheckerDist, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _pushPlatformDist);
    }

    public RaycastHit2D GroundCheck()
    {
        return Physics2D.BoxCast(_groundChecker.position, _groundCheckerSize, 0, -transform.up, _groundCheckerDist, _groundLayer);
    }

    private void TeleportTo(Vector3 position)
    {
        _networkRb.TeleportToPosition(position);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("DeathZone"))
        {
            TakeDamage();
        }

        if (Runner.IsServer)
        {
            Teleport teleport = collision.GetComponent<Teleport>();
            BuffScript buffScript = collision.GetComponent<BuffScript>();

            if (teleport != null && teleport.IsActive && !_resistSphere.IsActive)
            {
                TeleportTo(teleport.GetPosition());
            }

            if (buffScript != null)
            {
                Effect buff = buffScript.GetBuff(_effectManager);
                if(buff is InstantEffect)
                {
                    _effectManager.Apply((InstantEffect)buff);
                }
                else
                {
                    _effectManager.Apply((ContinuousEffect)buff);
                }
            }
        }
    }
}
