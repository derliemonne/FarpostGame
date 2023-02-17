using System;
using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkRigidbody2D))]
public class Character : NetworkBehaviour
{
    private const float HorizontalSpeedConsideredNotMoving = 0.1f;
    public int Health { get; private set; } = 1;
    public bool IsDead { get; private set; }

    public float Height => transform.position.y;
    public GameObject ResistSphere => _resistSphere;
    
    private static readonly int _isJumping = Animator.StringToHash("is_jumping");
    private static readonly int _isRunning = Animator.StringToHash("is_running");
    
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _jumpSpeed = 5f;
    [SerializeField] private Vector2 _groundCheckerSize;
    [SerializeField] private float _groundCheckerDist;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundChecker;
    [SerializeField] private NetworkMecanimAnimator _networkAnimator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private LevelGenerator _levelGenerator;
    [SerializeField] private GameObject _resistSphere;
    [SerializeField] private Effect?[] _effects = new Effect?[10];

    private NetworkRigidbody2D _networkRb;


    private void Awake()
    {
        _networkRb = GetComponent<NetworkRigidbody2D>();
    }

    public void TakeDamage()
    {
        Health--;
        if(Health <= 0)
        {
            Death();
        }
        else if (_levelGenerator != null)
        {
            transform.position = _levelGenerator.GetPlatformToPlace().position;
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
        if (GetInput(out NetworkInputData inputData))
        {
            Vector2 input = inputData.Direction.normalized;
            Vector2 velocity = _networkRb.Rigidbody.velocity;

            if(GroundCheck())
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
            velocity.x = input.x * _moveSpeed;

            _networkRb.Rigidbody.velocity = velocity;
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(
            new Vector3(_groundChecker.position.x, _groundChecker.position.y - _groundCheckerDist / 2), 
            new Vector3(_groundCheckerSize.x, _groundCheckerSize.y + _groundCheckerDist, 1));
    }

    private bool GroundCheck()
    {
        return Physics2D.BoxCast(_groundChecker.position, _groundCheckerSize, 0, -transform.up, _groundCheckerDist, _groundLayer);
    }
}
