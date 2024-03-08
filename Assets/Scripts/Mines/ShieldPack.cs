using Unity.Netcode;
using UnityEngine;

public class ShieldPack : NetworkBehaviour
{
    [SerializeField] private int _shieldAmount = 2;

   void OnTriggerEnter2D(Collider2D other)
   {
    if(!IsServer) return;

    Health health;
    if (!other.TryGetComponent<Health>(out health)) return;
    
    health.GainShieldCharges(_shieldAmount);

    NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
    networkObject.Despawn();
   }

}
