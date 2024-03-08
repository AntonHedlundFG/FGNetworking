using Unity.Netcode;
using UnityEngine;

public class HealthPack : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 50;

   void OnTriggerEnter2D(Collider2D other)
   {
    if(!IsServer) return;

    Health health;
    if (!other.TryGetComponent<Health>(out health)) return;
    
    health.HealDamage(_healAmount);

    NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
    networkObject.Despawn();
   }

}
