using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Damageable : NetworkBehaviour
{
    public int health;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    //public int health = 10;
    public bool canGetDeleted = true;

    private void Start() {
        currentHealth.Value = health;
    }
    private void Update() {
        if (currentHealth.Value <= 0)
        {
            if (canGetDeleted)
            {
                //NetworkObject.Despawn(true);
                Destroy(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetDamagedServerRpc(int damage)
    {
        currentHealth.Value -= damage;
    }
}
