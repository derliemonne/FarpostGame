using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GroundChecker : MonoBehaviour, IGroundChecker, IGroundGetter
{
    public bool OnGround => GetGroundColliders(out _);

    private BoxCollider2D _groundChecker;

    private void Awake()
    {
        _groundChecker = GetComponent<BoxCollider2D>();
    }

    public List<GroundInfo> GetGround()
    {
        List<Collider2D> collidersList;
        List<GroundInfo> groundInfos = new List<GroundInfo>();

        GetGroundColliders(out collidersList);
        foreach(var collider in collidersList)
        {
            Platform platform;
            CrateTop crateTop;

            if(collider.TryGetComponent(out platform))
            {
                groundInfos.Add(new GroundInfo { SkiCoef = platform.SkiCoefficient, IsCrateTop = false });
            }

            if(collider.TryGetComponent(out crateTop))
            {
                groundInfos.Add(new GroundInfo { SkiCoef = 1, IsCrateTop = true });
            }
        }
        return groundInfos;
    }

    public bool GetGroundColliders(out List<Collider2D> collidersList)
    {
        collidersList = new List<Collider2D>();
        if (_groundChecker == null)
        {
            Debug.LogError("Ground checker is null.");
            return false;
        }
        else
        {
            ContactFilter2D filter = new ContactFilter2D();
            _groundChecker.OverlapCollider(filter, collidersList);
            return collidersList.Count > 0;
        }    
    }
}
