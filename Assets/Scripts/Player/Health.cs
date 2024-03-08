using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<int> _shieldCharges = new NetworkVariable<int>();
    [SerializeField] private SpriteRenderer _shieldSprite;

    public override void OnNetworkSpawn()
    {
        _shieldCharges.OnValueChanged += UpdateShieldVisual;
        _shieldSprite.enabled = false;

        if(!IsServer) return;
        currentHealth.Value = _maxHealth;
        _shieldCharges.Value = 0;
    }


    public void TakeDamage(int damage){
        if (_shieldCharges.Value > 0)
        {
            _shieldCharges.Value--;
            return;
        }
        
        damage = Mathf.Abs(damage);
        currentHealth.Value -= damage;
    }

    public void HealDamage(int heal)
    {
        heal = Mathf.Abs(heal);
        currentHealth.Value += Mathf.Max(0, Mathf.Min(heal, _maxHealth - currentHealth.Value)); //Cannot heal above max health or negative values.

        if (ServerMessageUI.Instance)
        {
            UserData userData = SavedClientInformationManager.GetUserData(OwnerClientId);
            ServerMessageUI.Instance.DisplayMessage(userData.userName + " healed", 1.0f);
        }
    }

    public void GainShieldCharges(int charges)
    {
        _shieldCharges.Value += charges;

        if (ServerMessageUI.Instance)
        {
            UserData userData = SavedClientInformationManager.GetUserData(OwnerClientId);
            ServerMessageUI.Instance.DisplayMessage(userData.userName + " gained a shield", 1.0f);
        }
    }
    
    private void UpdateShieldVisual(int oldValue, int newValue)
    {
        if (!_shieldSprite) return;
        _shieldSprite.enabled = (newValue > 0);
    }

}
