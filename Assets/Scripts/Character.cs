using System;
using System.Collections;
using Fusion;
using UnityEngine;

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

    public float Height => transform.position.y;
    public GameObject ResistSphere => _resistSphere;
    public GameObject PlayerGameObject => _playerGameObject;

    //id игрока, который владеет персонажем, в будущем можно заменить на объект класса Player
    [Networked] public int PlayerId { get; private set; } = -1;

    private static readonly int _isJumping = Animator.StringToHash("is_jumping");
    private static readonly int _isRunning = Animator.StringToHash("is_running");
    
    [SerializeField] protected float _moveSpeed = 1f;
    [SerializeField] protected float _moveSpeedInAir = 0.8f;
    [SerializeField] protected float _jumpSpeed = 5f;
    [SerializeField] protected float _pushPlatformDist = 1f;
    [SerializeField] protected Vector2 _groundCheckerSize;
    [SerializeField] protected float _groundCheckerDist;
    [SerializeField] protected LayerMask _groundLayer;
    [SerializeField] protected LayerMask _crateTopLayer;
    [SerializeField] protected LayerMask _crateLayer;
    [SerializeField] protected Collider2D _groundChecker;
    [SerializeField] protected NetworkMecanimAnimator _networkAnimator;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected GameObject _resistSphere;
    [SerializeField] protected Effect?[] _effects = new Effect?[10];
    [SerializeField] protected GameObject _playerGameObject;
    [SerializeField] protected Transform _spriteTransform;

    private NetworkRigidbody2D _networkRb;

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

    public void TakeEffect(Effect effect)
    {
        if(!effect.IsInstant) 
        { 
            for(int i = 0; i < 10; i++)
            {
                if (_effects[i] == null)
                {
                    _effects[i] = effect;
                    effect.GiveEffect();
                    StartCoroutine(EffectDuration(effect.Duration, i));
                }
            }
        }
        else
        {
            effect.GiveEffect();
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        
        Vector2 direction = inputData.Direction.normalized;
        Vector2 velocity = _networkRb.Rigidbody.velocity;

        if(inputData.PushedPlatform)
        {
            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.down, _pushPlatformDist, _groundLayer);
            if(raycastHit.transform != null && raycastHit.transform.TryGetComponent(out Platform platform))
            {
                platform.PushPlatform();
            }                            
        }

        bool groundCheck = GroundCheck();
        if (groundCheck && direction.y > 0.1)
        {
            velocity.y = _jumpSpeed;
        }
        _networkAnimator.Animator.SetBool(_isJumping, !groundCheck);
        _networkAnimator.Animator.SetBool(_isRunning, Math.Abs(direction.x) > HorizontalSpeedConsideredNotMoving);
        velocity.x = direction.x * (groundCheck ? _moveSpeed : _moveSpeedInAir);

        _networkRb.Rigidbody.velocity = velocity;
    }

    private void Update()
    {
        if (_networkAnimator.Animator.GetBool(_isRunning))
        {
            _spriteRenderer.flipX = _networkRb.Rigidbody.velocity.x < 0;
        }
    }

    private IEnumerator EffectDuration(float duration, int num)
    {
        yield return new WaitForSeconds(duration);
        TakeOffEffect(num);
    }

    private void TakeOffEffect(int num)
    {
        _effects[num].TakeOffEffect();
        _effects[num] = null;
    }

    private void Death()
    {
        gameObject.SetActive(false);
        IsDead = true;
    }

    protected virtual void Stun()
    {
        Debug.Log("STUN!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _pushPlatformDist);
    }

    public bool GroundCheck()
    {
        if (_groundChecker == null)
        {
            Debug.LogError("Ground checker is null.");
            return false;
        }
        return _groundChecker.IsTouchingLayers(_groundLayer);
    }

    private bool LandOnTopOfCrate()
    {
        if (_groundChecker == null)
        {
            Debug.LogError("Ground checker is null.");
            return false;
        }
        return _groundChecker.IsTouchingLayers(_crateTopLayer);
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

        if (Runner.IsServer && TryGetComponent(out Teleport teleport) && teleport.IsActive)
        {
            TeleportTo(teleport.GetPosition());
        }
        
        if (Runner.IsServer && collision.TryGetComponent(out Crate crate) && !LandOnTopOfCrate())
        {
            Stun();
        }
    }
}
