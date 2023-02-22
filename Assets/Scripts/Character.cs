using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
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
    public GroundChecker GroundChecker => _groundChecker;

    //id игрока, который владеет персонажем, в будущем можно заменить на объект класса Player
    [Networked] public int PlayerId { get; private set; } = -1;

    private static readonly int _isJumping = Animator.StringToHash("is_jumping");
    private static readonly int _isRunning = Animator.StringToHash("is_moving");
    private static readonly int _isStunnedAnim = Animator.StringToHash("is_stunned");

    [SerializeField] protected float _moveSpeed = 1f;
    [SerializeField] protected float _moveSpeedInAir = 0.8f;
    [SerializeField] protected float _jumpSpeed = 5f;
    [SerializeField] protected float _pushPlatformDist = 1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] protected GroundChecker _groundChecker;
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

    public virtual void Stun(float duration)
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
        if (GetInput(out NetworkInputData inputData))
        {
            List<Platform> standOnPlatforms;
            bool isGrounded = _groundChecker.GetGroundPlatforms(out standOnPlatforms);

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

            if(isGrounded)
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

            Platform firstFrozenPlatform = null;
            foreach(Platform platform in standOnPlatforms)
            {
                if(platform.IsFrozen)
                {
                    firstFrozenPlatform = platform;
                    break;
                }
            }

            if(firstFrozenPlatform != null && firstFrozenPlatform.IsFrozen && !IceBoots.IsActive)
            {
                inputInter += input.x * firstFrozenPlatform.SkiCoefficient;
                inputInter -= 0.1f * firstFrozenPlatform.SkiCoefficient * Math.Sign(inputInter);
                inputInter = Math.Min(1, inputInter);
                inputInter = Math.Max(-1, inputInter);
            }
            else
            {
                inputInter = input.x;
            }

            velocity.x = inputInter * (isGrounded ? _moveSpeed : _moveSpeedInAir);

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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _pushPlatformDist);
    }

    private void TeleportTo(Vector3 position)
    {
        _networkRb.TeleportToPosition(position);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("DeathZone"))
        {
            TakeDamage();
        }

        if (Runner.IsServer)
        {
            if (collision.TryGetComponent(out BuffScript buffScript))
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

            if (collision.TryGetComponent(out Teleport teleport) && teleport.IsActive && !_resistSphere.IsActive)
            {
                TeleportTo(teleport.GetPosition());
            }

            if (collision.TryGetComponent(out Crate crate) && !_groundChecker.LandOnTopOfCrate() && !_resistSphere.IsActive)
            {
                Stun(5);
            }
        }  
    }
}
