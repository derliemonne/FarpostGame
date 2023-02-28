using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

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

    public static Dictionary<CharacterType, string> CharacterNames = new()
    {
        { CharacterType.Pirsik, "Пырсик" },
        { CharacterType.Firsik, "Фырсик" },
        { CharacterType.Marsik, "Марсик" },
        { CharacterType.Gull, "Чайка" }
    };

    /// <summary>
    /// Returns this character and altitude record.
    /// </summary>
    public event Action<Character, float> Died;
    
    /// <summary>
    /// Need override in each subclass.
    /// </summary>
    public abstract CharacterType CharacterType { get; }
    public abstract string CharacterName { get; }
    public int Health { get; protected set; } = 1;
    
    public bool IsDead
    {
        get => _isDead;
        set
        {
            _isDead = value;
            if (_isDead) Died?.Invoke(this, AltitudeRecord);
            Debug.Log("AltitudeRecord: " + AltitudeRecord);
        }
    }
    public bool IsStunned { get; private set; }

    public float Altitude => transform.position.y;
    public ResistSphere ResistSphere => _resistSphere;
    public IceBoots IceBoots => _iceBoots;
    public GroundChecker GroundChecker => _groundChecker;

    //id игрока, который владеет персонажем, в будущем можно заменить на объект класса Player
    [Networked] public int PlayerId { get; private set; } = -1;
    [Networked] public float AltitudeRecord { get; protected set; }

    protected static readonly int _isJumping = Animator.StringToHash("is_jumping");
    protected static readonly int _isRunning = Animator.StringToHash("is_moving");
    protected static readonly int _isStunnedAnim = Animator.StringToHash("is_stunned");

    [SerializeField] protected float _moveSpeed = 1f;
    [SerializeField] protected float _moveSpeedInAir = 0.8f;
    [SerializeField] protected float _jumpSpeed = 5f;
    [SerializeField] protected float _pushPlatformDist = 1f;
    [SerializeField] protected float _pushPlatformCooldown = 5f;
    [SerializeField] protected float _jumpCoolDown = 0.5f;
    [SerializeField] protected float _crateStunDuration = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] protected GroundChecker _groundChecker;
    [SerializeField] protected NetworkMecanimAnimator _networkAnimator;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected Transform _spriteTransform;
    [SerializeField] protected EffectManager _effectManager;
    [SerializeField] protected IceBoots _iceBoots;
    [SerializeField] protected ResistSphere _resistSphere;

    protected NetworkRigidbody2D _networkRb;
    protected float inputInter = 0;
    protected bool _isDead;

    [Networked] public bool CanJump { get; protected set; } = true;
    [Networked] protected bool _canPushPlatform { get; set; } = true;

    protected virtual void Awake()
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

    /// <summary>
    /// Server-only.
    /// </summary>
    protected virtual void TakeDamage()
    {
        if (!Runner.IsServer)
        {
            Debug.LogError("Server-only method on client.");
            return;
        }
        
        Health--;
        if(Health <= 0)
        {
            Rpc_Death();
        }
    }

    public void Stun(float duration)
    {
        Rpc_SetActiveStun(true);
        StartCoroutine(StunDuration(duration));
    }

    public void PermanentStun()
    {
        Rpc_SetActiveStun(true);
    }

    protected virtual void CrateStun(float duration)
    {
        Stun(duration);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void Rpc_SetActiveStun(bool value)
    {
        IsStunned = value;
        _networkAnimator.Animator.SetBool(_isStunnedAnim, value);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_Death()
    {
        gameObject.SetActive(false);
        IsDead = true;
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
            int moveDirection = inputData.Direction;
            bool jump = inputData.Jumped;
            bool pushPlatform = inputData.PushedPlatform;
            if (pushPlatform) print("PUSH PLATFORM");
            Vector2 velocity = _networkRb.Rigidbody.velocity;

            if(IsStunned)
            {
                moveDirection = 0;
                jump = false;
            }

            if(inputData.PushedPlatform && !IsStunned && _canPushPlatform)
            {
                PushPlatform();
                _canPushPlatform = false;
                StartCoroutine(CooldownPushPlatform());
            }

            Move(moveDirection, velocity, jump);
            AltitudeRecord = Mathf.Max(AltitudeRecord, Altitude);
        }
    }

    private IEnumerator CooldownPushPlatform()
    {
        yield return new WaitForSeconds(_pushPlatformCooldown);
        _canPushPlatform = true;
    } 
    private void PushPlatform()
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

    protected virtual void Move(int moveDirection, Vector3 velocity, bool jump)
    {
        List<Platform> standOnPlatforms;
        bool isGrounded = _groundChecker.GetGroundPlatforms(out standOnPlatforms);

        Jump(jump, ref velocity, isGrounded);

        _networkAnimator.Animator.SetBool(_isRunning, Math.Abs(moveDirection) > HorizontalSpeedConsideredNotMoving);

        Platform firstFrozenPlatform = null;
        foreach (Platform platform in standOnPlatforms)
        {
            if (platform.IsFrozen)
            {
                firstFrozenPlatform = platform;
                break;
            }
        }

        if (firstFrozenPlatform != null && firstFrozenPlatform.IsFrozen && !IceBoots.IsActive)
        {
            inputInter += moveDirection * firstFrozenPlatform.SkiCoefficient;
            inputInter -= 0.1f * firstFrozenPlatform.SkiCoefficient * Math.Sign(inputInter);
            inputInter = Math.Min(1, inputInter);
            inputInter = Math.Max(-1, inputInter);
        }
        else
        {
            inputInter = moveDirection;
        }

        velocity.x = inputInter * (isGrounded ? _moveSpeed : _moveSpeedInAir);

        _networkRb.Rigidbody.velocity = velocity;
    }

    protected virtual void Jump(bool jump, ref Vector3 velocity, bool isGrounded)
    {
        if (isGrounded)
        {
            if (jump && CanJump)
            {
                velocity.y = _jumpSpeed;
                ResetJump(_jumpCoolDown);
            }
            _networkAnimator.Animator.SetBool(_isJumping, false);
        }
        else
        {
            _networkAnimator.Animator.SetBool(_isJumping, true);
        }
    }

    protected void ResetJump(float duration)
    {
        CanJump = false;
        StartCoroutine(ResetJumpDelay(duration));
    }

    private IEnumerator ResetJumpDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        CanJump = true;
    }

    private void Update()
    {
        if (_networkAnimator.Animator.GetBool(_isRunning) && 
            Math.Abs(_networkRb.Rigidbody.velocity.x) > 0.01)
        {
            _spriteRenderer.flipX = _networkRb.Rigidbody.velocity.x < 0;
        }
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
        if (Runner.IsServer)
        {
            if (collision.TryGetComponent(out Darkness darkness))
            {              
                TakeDamage();
            }

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
                CrateStun(_crateStunDuration);
            }
        }  
    }
}
