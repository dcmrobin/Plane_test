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
            GenerateWings(parentObject);
        }
    }

    public void GenerateWings(GameObject plane)
    {
        PlanePilot playerScript = plane.GetComponent<PlanePilot>();

        GameObject spawnedwings = Instantiate(wingsPrefab, Vector3.zero, Quaternion.identity);
        spawnedwings.GetComponent<NetworkObject>().SpawnWithOwnership(plane.GetComponent<NetworkObject>().OwnerClientId);
        spawnedwings.transform.SetParent(plane.transform);
        //playerScript.curwings = spawnedwings;

        GameObject spawnedtailfinLow = Instantiate(tailfinLowPrefab, Vector3.zero, Quaternion.identity);
        spawnedtailfinLow.GetComponent<NetworkObject>().SpawnWithOwnership(plane.GetComponent<NetworkObject>().OwnerClientId);
        spawnedtailfinLow.transform.SetParent(plane.transform);
        //playerScript.curtailLow = spawnedtailfinLow;

        GameObject spawnedtailfinHigh = Instantiate(tailfinHighPrefab, Vector3.zero, Quaternion.identity);
        spawnedtailfinHigh.GetComponent<NetworkObject>().SpawnWithOwnership(plane.GetComponent<NetworkObject>().OwnerClientId);
        spawnedtailfinHigh.transform.SetParent(plane.transform);
        //playerScript.curtailHigh = spawnedtailfinHigh;
        playerScript.AssignClientRpc();
    }
}
