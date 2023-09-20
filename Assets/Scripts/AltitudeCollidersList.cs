using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AltitudeCollidersList : MonoBehaviour
{
    public static AltitudeCollidersList Instance { get; private set; }

    private List<AltitudeCollider> _altitudeCollidersList = new List<AltitudeCollider>();

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    public void AddAltitudeCollider(AltitudeCollider altitudeCollider)
    {
        _altitudeCollidersList.Add(altitudeCollider);
    }

    public List<AltitudeCollider> GetTriggeredAltitudeColliders(float altitude, bool higher)
    {
        return _altitudeCollidersList.Where(altitudeCollider => higher ?
            altitudeCollider.GetAltitude() > altitude :
            altitudeCollider.GetAltitude() < altitude).
            ToList();
    }
}
