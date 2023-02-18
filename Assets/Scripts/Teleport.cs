using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : NetworkBehaviour
{
    [SerializeField] private Transform _anotherTp;

    public Vector3 GetPosition()
    {
        return _anotherTp.transform.position;
    }

    public bool IsActive => _anotherTp != null;
}
