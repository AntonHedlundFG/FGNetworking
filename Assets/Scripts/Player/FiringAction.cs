using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FiringAction : NetworkBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject clientSingleBulletPrefab;
    [SerializeField] GameObject serverSingleBulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;

    private NetworkVariable<bool> _isShooting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private float _lastShootTime = 0.0f; //Used by client and server, separately.
    [SerializeField][Range(0.1f, 10.0f)] private float _shootingCooldownSeconds = 1.0f; 

    public override void OnNetworkSpawn()
    {
        playerController.onFireEvent += Fire;
    }

    private void Fire(bool isShooting)
    {
        _isShooting.Value = isShooting;
    }

    private void Update()
    {
        if (!_isShooting.Value) return;
        if (Time.time < _lastShootTime + _shootingCooldownSeconds) return; //Still on cooldown

        if (IsOwner)
        {
            ShootLocalBullet(); //Fire predictive shot if local cooldown is up.
        }
        if (IsServer)
        {
            //Fire actual shot and notify other clients if server cooldown is up.
            ShootServerBullet(); 
            ShootLocalBulletClientRpc();
        }

        _lastShootTime = Time.time;
    }

    [ClientRpc]
    private void ShootLocalBulletClientRpc()
    {
        if (IsOwner) return; //Owner has hopefully already fired a predictive shot.
        ShootLocalBullet();
    }

    private void ShootLocalBullet()
    {
        GameObject bullet = Instantiate(clientSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
    }

    private void ShootServerBullet()
    {
        GameObject bullet = Instantiate(serverSingleBulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), transform.GetComponent<Collider2D>());
    }
}
