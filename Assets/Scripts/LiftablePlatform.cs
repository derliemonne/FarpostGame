using Fusion;
using UnityEngine;

public class LiftablePlatform : NetworkBehaviour
{
    [SerializeField] private bool _isMoving = true;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _travelDistance = 128f / 100f * 3;
    [SerializeField] private float _secondsPlatformNotMoving = 1f;

    private Vector2 _startPosition;

    public override void Spawned()
    {
        _startPosition = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (!_isMoving) return;
        int currentTick = Runner.Tick.Raw;
        float absoluteTime = currentTick * Runner.DeltaTime;
        // Platform cycle: not move - move up - not move - move down
        // Not move lasts t1 seconds.
        // Move up (down) lasts t2 seconds.
        float t1 = _secondsPlatformNotMoving;
        float t2 = _travelDistance / _speed;
        float fullCycleTime = (t1 + t2) * 2;
        float t = absoluteTime % fullCycleTime;
        float deltaY;
        // Not move.
        if (t < t1)
        {
            deltaY = 0;
        }
        // Move up.
        else if (t < t1 + t2)
        {
            deltaY = Mathf.Lerp(0, _travelDistance, (t - t1) / t2);
        }
        // Not move.
        else if (t < t1 + t2 + t1)
        {
            deltaY = _travelDistance;
        }
        // Move down.
        else
        {
            deltaY = Mathf.Lerp(_travelDistance, 0, (t - t1 - t2 - t1) / t2);
        }
        transform.position = _startPosition + Vector2.up * deltaY;
    }
}
