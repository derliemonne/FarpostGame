using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public event Action<bool> CanJumpChanged;

    /// <summary>
    /// Need override in each subclass.
    /// </summary>
    public abstract CharacterType CharacterType { get; }
    public abstract string CharacterName { get; }
    public float Altitude => transform.position.y;

    //id игрока, который владеет персонажем, в будущем можно заменить на объект класса Player
    [Networked] public int PlayerId { get; private set; } = -1;
    [Networked] public float AltitudeRecord { get; protected set; }

    protected static readonly int _isJumping = Animator.StringToHash("is_jumping");
    protected static readonly int _isRunning = Animator.StringToHash("is_moving");
    protected static readonly int _isStunnedAnim = Animator.StringToHash("is_stunned");

    [SerializeField] protected GroundChecker _groundChecker;
    public GroundChecker GroundChecker => _groundChecker;

    [SerializeField] protected NetworkMecanimAnimator _networkAnimator;
    public NetworkMecanimAnimator NetworkAnimator => _networkAnimator;

    [SerializeField] protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    [SerializeField] protected Transform _spriteTransform;
    public Transform SpriteTransform => _spriteTransform;

    [SerializeField] protected EffectManager _effectManager;
    public EffectManager EffectManager => _effectManager;   

    [SerializeField] private Transform _playersLegsTransform;
    public Transform PlayersLegsTransform => _playersLegsTransform;

    [SerializeField] private Transform _playersBodyTransform;
    public Transform PlayersBodyTransform => _playersBodyTransform;

    [SerializeField] private HealthScript _healthScript;
    public HealthScript HealthScript => _healthScript;

    [SerializeField] private CharacterMoveScript _characterMoveScript;
    public CharacterMoveScript CharacterMoveScript => _characterMoveScript;

    [SerializeField] private PushPlatformScript _pushPlatformScript;
    public PushPlatformScript PushPlatformScript => _pushPlatformScript;


    [Header("Sounds")]
    [SerializeField] protected AudioClip _deathSound;
    [SerializeField] protected AudioClip _jumpSound;

    protected NetworkRigidbody2D _networkRb;
    protected AudioSource _audioSource;
    [SerializeField] protected PlayerSound _playerSound;

    protected virtual void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody2D>();  
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetPlayerId(int playerId)
    {
        if(PlayerId == -1)
        {
            PlayerId = playerId;
        }
    }

    public void BindPlayerSound(PlayerSound playerSound)
    {
        _playerSound = playerSound;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData inputData))
        {
            AltitudeRecord = Mathf.Max(AltitudeRecord, Altitude);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _pushPlatformDist);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (Runner.IsServer)
        {
            if (collision.TryGetComponent(out Darkness darkness))
            {
                _healthScript.TakeDamage();
            }

            if (collision.TryGetComponent(out BuffScript buffScript))
            {
                Effect buff = buffScript.GetBuff(_effectManager);
                
                if (_playerSound != null)
                {
                    Rpc_PlayBuff(PlayerId);
                }
                else
                {
                    Debug.LogError("_playerSound is null");
                }

                if (buff is InstantEffect)
                {
                    _effectManager.Apply((InstantEffect)buff);
                }
                else
                {
                    _effectManager.Apply((ContinuousEffect)buff);
                }
            }

            if (collision.TryGetComponent(out Teleport teleport) && teleport.IsActive && _effectManager.HasEffect(typeof(ResistEffect)))
            {
                Vector3? tpPosition = teleport.GetNextPosition();

                if (!tpPosition.HasValue)
                {
                    _healthScript.TakeDamage();
                    return;
                }

                if(tpPosition.Value.y <= Darkness.Instance.Altitude)
                {
                    _healthScript.TakeDamage();
                    return;
                }

                _effectManager.Apply(new ResistEffect(_effectManager, 1));
                _effectManager.Apply(new TpEffect(_effectManager, transform, (Vector3)tpPosition));
            }

            bool landOnTopOfCrate = _groundChecker.GetGround().Where(groundInfo => groundInfo.IsCrateTop).Count() > 0;

            if (collision.TryGetComponent(out Crate crate) && !landOnTopOfCrate && _effectManager.HasEffect(typeof(ResistEffect)))
            {
                _effectManager.Apply(new StunEffect(_effectManager, this, 4));
            }
        }  
    }
   
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    protected void Rpc_PlayDeath([RpcTarget] PlayerRef player)
    {
        _playerSound.Play_Death();
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    protected void Rpc_PlayBuff([RpcTarget] PlayerRef player)
    {
        _playerSound.Play_Buff();
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    protected void Rpc_PlayJump([RpcTarget] PlayerRef player)
    {
        _playerSound.Play_Jump();
    }
}
