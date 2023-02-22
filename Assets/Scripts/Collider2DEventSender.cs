using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collider2DEventSender : MonoBehaviour
{
    public Collider2D Collider => _collider;

    public event Action<Collision2D> CollisionEnter2D;
    public event Action<Collision2D> CollisionStay2D;
    public event Action<Collision2D> CollisionExit2D;
    public event Action<Collider2D> TriggerEnter2D;
    public event Action<Collider2D> TriggerStay2D;
    public event Action<Collider2D> TriggerExit2D;
    
    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CollisionEnter2D?.Invoke(other);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        CollisionStay2D?.Invoke(other);
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        CollisionExit2D?.Invoke(other);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerEnter2D?.Invoke(other);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        TriggerStay2D?.Invoke(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TriggerExit2D?.Invoke(other);
    }
}
