using System;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public event Action OnDeath;
    public int CurHealth { get; private set; } = 1;
    public bool UnderResist { get; set; } = false;

    public void TakeDamage()
    {
        if(!UnderResist)
        {
            CurHealth--;
            if(CurHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }   
    }

    public void Heal(int hp)
    {
        CurHealth += hp;
    }

    public void Kill()
    {
        CurHealth = 0;
        OnDeath?.Invoke();
    }
}
