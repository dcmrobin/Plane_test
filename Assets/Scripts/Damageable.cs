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
        AssignHealthServerRpc(health);
    }
    private void Update() {
        if (currentHealth.Value <= 0)
        {
            if (canGetDeleted)
            {
                //NetworkObject.Despawn(true);
                DespawnObjectServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetDamagedServerRpc(int damage)
    {
        currentHealth.Value -= damage;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignHealthServerRpc(int amount)
    {
        currentHealth.Value = amount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnObjectServerRpc()
    {
        NetworkObject.Despawn(true);
    }
}
