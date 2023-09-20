using System;
using UnityEngine;

public abstract class DamageZone : MonoBehaviour
{
    private event Action<HealthScript> OnTakeDamage;

    private void Awake()
    {
        OnTakeDamage += (healthScript) =>
        {
            healthScript.TakeDamage();
        };
    }

    /// <summary>
    /// Must define conditions when health script is damaged and then MUST invoke OnTakeDamage event
    /// </summary>
    protected abstract void ConditionToDamage();
}
