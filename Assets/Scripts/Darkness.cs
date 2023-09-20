using Fusion;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Darkness : NetworkBehaviour
{
    public static Darkness Instance { get; private set; }

    [SerializeField] private MoveScript _moveScript;
    [SerializeField] private DamageZone _damageZoneScript;
    
    [Networked] public float Altitude { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public float Speed => _speed;

    [SerializeField] private float _speed = 1;
    
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider2D;
    private Transform _transform;

    public void SetActive(bool value)
    {
        IsActive = value;
        Rpc_SetActivateDeathZone(value);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_SetActivateDeathZone(bool value)
    {
        _spriteRenderer.enabled = value;
    }

    public override void Spawned()
    {
        Debug.Log("Darkness spawned");
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        
        _transform = gameObject.GetComponent<Transform>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (IsActive)
        {
            Move(Runner.DeltaTime);
        }
        _transform.position = new Vector3(0, Altitude - _boxCollider2D.size.y / 2, 0);
        // Debug.Log(IsActive + " : " + _deathZoneRenderer.enabled + " : " + _deathZoneTr.position.y);
    }

    private void Move(float deltaSeconds)
    {
        if(Runner.IsServer)
        {
            Altitude += Speed * deltaSeconds;
        }
    }

    private void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
