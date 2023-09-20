using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AltitudeCollider))]
public class AltitudeTrigger : NetworkBehaviour
{
    private static List<AltitudeCollider> _allAltitudeCollidersOnScene = new List<AltitudeCollider>();

    public event Action<AltitudeCollider> Enter;
    public event Action<AltitudeCollider> Stay;
    public event Action<AltitudeCollider> Exit;

    public AltitudeCollider ThisAltitudeCollider { get; private set; } 

    public bool IsUpper { get; private set; } = false;

    private List<AltitudeCollider> _prevIntersectedAltitudeColliders = new List<AltitudeCollider>();

    private void Awake()
    {
        ThisAltitudeCollider = GetComponent<AltitudeCollider>();
    }

    public override void FixedUpdateNetwork()
    {
        List<AltitudeCollider> exceptList = new List<AltitudeCollider>
        {
            ThisAltitudeCollider
        };

        List<AltitudeCollider> curAltitudeCollidersList = _allAltitudeCollidersOnScene.
            Where(altCol => IsUpper ? altCol.GetAltitude() > ThisAltitudeCollider.GetAltitude() :
                altCol.GetAltitude() < ThisAltitudeCollider.GetAltitude()).
            Except(exceptList).
            ToList();

        List<AltitudeCollider> enteredAltitudeCollidersList = curAltitudeCollidersList.
            Except(_prevIntersectedAltitudeColliders).ToList();

        List<AltitudeCollider> exitedAltitudeCollidersList = _prevIntersectedAltitudeColliders.
            Except(curAltitudeCollidersList).ToList();

        foreach (var enteredAltitudeCollider in enteredAltitudeCollidersList)
        {
            Enter?.Invoke(enteredAltitudeCollider);
        }
        foreach (var stayingAltitudeCollider in curAltitudeCollidersList)
        {
            Stay?.Invoke(stayingAltitudeCollider);
        }
        foreach (var exitedAltitudeCollider in exitedAltitudeCollidersList)
        {
            Exit?.Invoke(exitedAltitudeCollider);
        }

        _prevIntersectedAltitudeColliders = curAltitudeCollidersList;
    }

    public static void AddToAltitudeCollidersList(AltitudeCollider altCol)
    {
        _allAltitudeCollidersOnScene.Add(altCol);
    }
}
