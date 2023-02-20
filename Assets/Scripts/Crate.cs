using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkRigidbody2D), typeof(BoxCollider2D))]
public class Crate : NetworkBehaviour
{
    [SerializeField] private float _fallingSpeed = Chunk.ChunkHeight / 8;
    [SerializeField, Range(0, 1)] private float _fallingSpeedMaxDeviation = 0.3f;
    [SerializeField] private float _destructionAnimationSeconds = 1f;
    [SerializeField] private Animator _animator;

    private NetworkRigidbody2D _networkRb;
    private BoxCollider2D _collider;
    
    public override void Spawned()
    {
        _networkRb = GetComponent<NetworkRigidbody2D>();
        _networkRb.Rigidbody.velocity = Vector2.down * _fallingSpeed * (1 + Random.Range(0, _fallingSpeedMaxDeviation));
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isServer = NetworkManager.Instance.NetworkRunner.IsServer;
        if (isServer && other.TryGetComponent(out Darkness darkness))
        {
            NetworkManager.Instance.NetworkRunner.Despawn(Object);
        }
        else if (other.TryGetComponent(out Character character))
        {
            if (character is CatScript) return;
            if (isServer)
            {
                NetworkManager.Instance.Despawn(Object, _destructionAnimationSeconds);
            }
            _collider.enabled = false;
            _animator.enabled = true;
        }
    }
}
