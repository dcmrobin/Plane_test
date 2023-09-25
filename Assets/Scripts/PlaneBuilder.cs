using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlaneBuilder : MonoBehaviour
{
    [SerializeField] private GameObject wingsPrefab;
    [SerializeField] private GameObject tailfinLowPrefab;
    [SerializeField] private GameObject tailfinHighPrefab;
    private void Start() {
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
            spawnedwings.transform.position = playerScript.wings.transform.position;
            spawnedwings.transform.rotation = playerScript.wings.transform.rotation;
            Destroy(playerScript.wings);
            playerScript.wings = spawnedwings;

            GameObject spawnedtailfinLow = Instantiate(tailfinLowPrefab, Vector3.zero, Quaternion.identity);
            spawnedtailfinLow.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            spawnedtailfinLow.transform.SetParent(parentObject.transform);
            spawnedtailfinLow.transform.position = playerScript.tailfinLow.transform.position;
            spawnedtailfinLow.transform.rotation = playerScript.tailfinLow.transform.rotation;
            Destroy(playerScript.tailfinLow);
            playerScript.tailfinLow = spawnedtailfinLow;

            GameObject spawnedtailfinHigh = Instantiate(tailfinHighPrefab, Vector3.zero, Quaternion.identity);
            spawnedtailfinHigh.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            spawnedtailfinHigh.transform.SetParent(parentObject.transform);
            spawnedtailfinHigh.transform.position = playerScript.tailfinHigh.transform.position;
            spawnedtailfinHigh.transform.rotation = playerScript.tailfinHigh.transform.rotation;
            Destroy(playerScript.tailfinHigh);
            playerScript.tailfinHigh = spawnedtailfinHigh;
        }
    }
}
