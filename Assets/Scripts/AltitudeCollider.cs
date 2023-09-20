using UnityEngine;

public class AltitudeCollider : MonoBehaviour, IAltitude
{
    [SerializeField] protected Transform _altitudeTransform;

    private void Start()
    {
        AltitudeTrigger.AddToAltitudeCollidersList(this);
    }

    public float GetAltitude()
    {
        return _altitudeTransform.position.y;
    }
}
