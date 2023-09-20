using UnityEngine;
using Fusion;

public class MoveScript : NetworkBehaviour
{
    [SerializeField] protected Rigidbody2D _movableRigidbody;
    [SerializeField] private IMoveController _moveController;

    public override void FixedUpdateNetwork()
    {
        Move();
    }
    protected virtual void Move()
    {
        Vector2 velocity = _movableRigidbody.velocity;
        velocity += _moveController.Direction * _moveController.Speed;
        _movableRigidbody.velocity = velocity;
    }
}
