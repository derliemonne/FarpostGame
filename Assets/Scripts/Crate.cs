using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkRigidbody2D), typeof(BoxCollider2D))]
public class Crate : NetworkBehaviour
{
    [SerializeField] private float _fallingSpeed = Chunk.ChunkHeight / 8;
    [SerializeField, Range(0, 1)] private float _fallingSpeedMaxDeviation = 0.3f;

    private NetworkRigidbody2D _networkRb;
    
    public override void Spawned()
    {
        _networkRb = GetComponent<NetworkRigidbody2D>();
        _networkRb.Rigidbody.velocity = Vector2.down * _fallingSpeed * (1 + Random.Range(0, _fallingSpeedMaxDeviation));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkManager.Instance.NetworkRunner.IsServer && other.TryGetComponent(out Darkness darkness))
        {
            NetworkManager.Instance.NetworkRunner.Despawn(Object);
        }
    }
}
