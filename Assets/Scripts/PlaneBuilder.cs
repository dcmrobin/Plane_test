using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlaneBuilder : NetworkBehaviour
{
    [SerializeField] private GameObject wingsPrefab;
    [SerializeField] private GameObject tailfinLowPrefab;
    [SerializeField] private GameObject tailfinHighPrefab;
    private void Awake() {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject parentObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            PlanePilot playerScript = parentObject.GetComponent<PlanePilot>();

            GameObject spawnedwings = Instantiate(wingsPrefab, Vector3.zero, Quaternion.identity);
            spawnedwings.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            spawnedwings.transform.SetParent(parentObject.transform);
            //playerScript.curwings = spawnedwings;

            GameObject spawnedtailfinLow = Instantiate(tailfinLowPrefab, Vector3.zero, Quaternion.identity);
            spawnedtailfinLow.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            spawnedtailfinLow.transform.SetParent(parentObject.transform);
            //playerScript.curtailLow = spawnedtailfinLow;

            GameObject spawnedtailfinHigh = Instantiate(tailfinHighPrefab, Vector3.zero, Quaternion.identity);
            spawnedtailfinHigh.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            spawnedtailfinHigh.transform.SetParent(parentObject.transform);
            //playerScript.curtailHigh = spawnedtailfinHigh;
            playerScript.AssignClientRpc();
        }
    }
}
