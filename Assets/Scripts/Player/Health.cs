using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        currentHealth.Value = _maxHealth;
    }


    public void TakeDamage(int damage){
        damage = Mathf.Abs(damage);
        currentHealth.Value -= damage;
    }

    public void HealDamage(int heal)
    {
        heal = Mathf.Abs(heal);
        currentHealth.Value += Mathf.Max(0, Mathf.Min(heal, _maxHealth - currentHealth.Value)); //Cannot heal above max health or negative values.
    }

}
