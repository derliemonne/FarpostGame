using Fusion;
using System.Data.SqlTypes;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Teleport : NetworkBehaviour
{
    public bool IsActive { get; private set; } = false;
    public float Altitude => transform.position.y;

    private Teleport _anotherTp;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetNextTp(Teleport teleport)
    {
        IsActive = true;
        _animator.enabled = true;
        _anotherTp = teleport;
    }

    public Vector3? GetNextPosition()
    {
        if(!IsActive)
        {
            Debug.LogError("This teleport is not active");
            return null;
        }

        if(_anotherTp != null)
        {
            return _anotherTp.transform.position;
        }
        return null;
    }
}
