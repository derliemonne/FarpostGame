using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DarknessAltitudeCollider : AltitudeCollider
{
    private BoxCollider2D _darknessBoxCollider;

    private void Awake()
    {
        _darknessBoxCollider = GetComponent<BoxCollider2D>();
    }

    public new float GetAltitude()
    {
        return _altitudeTransform.position.y + _darknessBoxCollider.size.y / 2;
    }
}
