using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Damageable : NetworkBehaviour
{
    public float health = 10;
    public bool canGetDeleted = true;

    private void Update() {
        if (health <= 0)
        {
            if (canGetDeleted)
            {
                Destroy(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetDamagedServerRpc(float damage)
    {
        health -= damage;
    }
}
