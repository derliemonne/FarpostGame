using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public bool IsGrounded => GetGroundColliders(out _);

    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _crateTopLayer;

    private BoxCollider2D _groundChecker;

    private void Awake()
    {
        _groundChecker = GetComponent<BoxCollider2D>();
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
            filter.SetLayerMask(_groundLayer);
            _groundChecker.OverlapCollider(filter, collidersList);
            return collidersList.Count > 0;
        }    
    }

    //method will return true if there id intersect with colliders from layer ground, 
    //platform script isnt necessary
    public bool GetGroundPlatforms(out List<Platform> platformsList)
    {
        List<Collider2D> collidersList;
        platformsList = new List<Platform>();
        GetGroundColliders(out collidersList);
        foreach(var collider in collidersList)
        {
            Platform platform;
            if(collider.TryGetComponent(out platform))
            {
                platformsList.Add(platform);
            }
        }
        return collidersList.Count > 0;
    }

    public bool LandOnTopOfCrate()
    {
        return _groundChecker.IsTouchingLayers(_crateTopLayer);
    }
}
