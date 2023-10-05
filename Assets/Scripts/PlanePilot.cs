using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using Unity.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    [Header("Pilot params")]
    public float speed = 10.0f;
    [Header("UI")]
    public GameObject statusTitle;
    public GameObject stalledText;
    public GameObject shotdownText;
    public GameObject crashedText;
    public GameObject respawnButton;
    public GameObject LockGuide;
    [Header("Gun params")]
    public int gunDamage = 2;
    public LayerMask targetMask;
    public Image crosshair;
    public Sprite nottargetedSprite;
    public Sprite targetedSprite;
    [Header("Plane status")]
    public bool crashed;
    public bool stalled;
    public bool shotDown;
    public bool slowingDown;
    public bool lockedOn;
    RaycastHit hit;
    [Header("Bits of the plane")]
    public GameObject body;
    public GameObject wings;
    public GameObject tailfinLow;
    public GameObject tailfinHigh;
    public GameObject curwings;
    public GameObject curtailLow;
    public GameObject curtailHigh;
    public GameObject cockpitCam;
    private Camera mainCam;

    private GameObject[] allPlayers;
    private List<GameObject> closestPlayers = new List<GameObject>();
    private int index = 0;

    private void Start() {
        mainCam = Camera.main;
        //GameObject.Find("PlaneBuilder").GetComponent<PlaneBuilder>().GenerateWings(gameObject);
        CheckForWings();
        Invoke("CheckForWings", 2);
    }
    void CheckForWings()
    {
        if (curwings == null && curtailLow == null && curtailHigh ==  null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                GameObject.Find("PlaneBuilder").GetComponent<PlaneBuilder>().GenerateWings(gameObject);
            }
            AssignClientRpc();
        }
    }
    [ClientRpc]
    public void AssignClientRpc()
    {
        crashed = false;
        stalled = false;
        shotDown = false;

        curwings = transform.Find(wings.name + "(Clone)").gameObject;
        curtailLow = transform.Find(tailfinLow.name + "(Clone)").gameObject;
        curtailHigh = transform.Find(tailfinHigh.name + "(Clone)").gameObject;

        curwings.transform.position = wings.transform.position;
        //Destroy(wings);
        wings = curwings;
    
        curtailLow.transform.position = tailfinLow.transform.position;
        //Destroy(tailfinLow);
        tailfinLow = curtailLow;
    
        curtailHigh.transform.position = tailfinHigh.transform.position;
        curtailHigh.transform.rotation = tailfinHigh.transform.rotation;
        //Destroy(tailfinHigh);
        tailfinHigh = curtailHigh;
    }
    private void FixedUpdate() {
        if (!IsOwner) return;

        Vector3 moveCamTo = transform.position - transform.forward * 10.0f + Vector3.up * 5.0f;
        float bias = 0.96f;
        if (mainCam.enabled)
        {
            mainCam.transform.position = mainCam.transform.position * bias + moveCamTo * (1.0f - bias);
            mainCam.transform.LookAt(transform.position + transform.forward * 30.0f);
        }

        if (!crashed && !shotDown)
        {
            if (!stalled)
            {
                transform.position += transform.forward * Time.deltaTime * speed;
            }
            if (!slowingDown)
            {
                speed -= transform.forward.y * Time.deltaTime * 50.0f;
            }
            //transform.Rotate(Input.GetAxis("Vertical") * 2, 0.0f, -Input.GetAxis("Horizontal") * 2);
            if (tailfinLow != null)
            {
                transform.Rotate(Input.GetAxis("Vertical") * 2, 0.0f, 0.0f);// Pitch
            }
            if (wings != null)
            {
                transform.Rotate(0.0f, 0.0f, -Input.GetAxis("Horizontal") * 2);// Roll
            }
            if (tailfinHigh != null)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.Rotate(0.0f, -1, 0.0f);// Yaw
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    transform.Rotate(0.0f, 1, 0.0f);// Yaw
                }
            }

            /*if (Input.GetKey(KeyCode.Space))
            {
                slowingDown = true;
                speed -= 1;
                if (speed <= 0)
                {
                    speed = 0;
                }
                GetComponent<Rigidbody>().isKinematic = false;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                slowingDown = false;
                GetComponent<Rigidbody>().isKinematic = true;
            }*/
        }

        if (speed <= 0.0f)
        {
            stalled = true;

            //speed = 35.0f;//not realistic... the engine should stall
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed/2, ForceMode.Impulse);
            speed = 0.0f;
        }
        else
        {
            stalled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Update() {
        if (body.GetComponent<Damageable>().currentHealth.Value <= 0)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed/2, ForceMode.Impulse);
            shotDown = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetMask))
        {
            crosshair.sprite = targetedSprite;
        }
        else
        {
            crosshair.sprite = nottargetedSprite;
        }

        // Toggle cockpit cam
        if (IsOwner)
        {
            if (!cockpitCam.activeSelf && Input.GetKeyDown(KeyCode.F1))
            {
                cockpitCam.SetActive(true);
                mainCam.enabled = false;
            }
            else if (cockpitCam.activeSelf && Input.GetKeyDown(KeyCode.F1))
            {
                cockpitCam.SetActive(false);
                mainCam.enabled = true;
                for (int i = 0; i < allPlayers.Length; i++)
                {
                    allPlayers[i].GetComponent<Outline>().enabled = false;
                }
            }
        }

        UpdateStatusText();
        if (cockpitCam.activeSelf)
        {
            UpdateLock();
        }
    }

    public void Fire()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetMask))
        {
            hit.collider.GetComponent<Damageable>().GetDamagedServerRpc(gunDamage);
        }
    }

    public void UpdateStatusText()
    {
        if (stalled)
        {
            stalledText.SetActive(true);
        }
        else if (shotDown)
        {
            shotdownText.SetActive(true);
        }
        else if (crashed)
        {
            respawnButton.SetActive(true);
            crashedText.SetActive(true);
        }
        else if (!stalled)
        {
            stalledText.SetActive(false);
        }
        else if (!shotDown)
        {
            shotdownText.SetActive(false);
        }
        else if (!crashed)
        {
            crashedText.SetActive(false);
        }

        if (!stalled && !shotDown && !crashed)
        {
            statusTitle.SetActive(false);
        }
        else
        {
            statusTitle.SetActive(true);
        }
    }

    public void UpdateLock()
    {
        float maxDist = 1000;
        if (!lockedOn)
        {
            allPlayers = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i] != gameObject)
                {
                    if (Vector3.Distance(allPlayers[i].transform.position, transform.position) < maxDist)
                    {
                        if (!closestPlayers.Contains(allPlayers[i]))
                        {
                            closestPlayers.Add(allPlayers[i]);
                        }
                    }
                }
            }
            for (int i = 0; i < closestPlayers.Count; i++)
            {
                if (Vector3.Distance(closestPlayers[i].transform.position, transform.position) > maxDist)
                {
                    if (closestPlayers[i].GetComponent<Outline>() != null)
                    {
                        closestPlayers[i].GetComponent<Outline>().enabled = false;
                    }
                    else
                    {
                        closestPlayers[i].AddComponent<Outline>().enabled = false;
                    }
                    closestPlayers.Remove(closestPlayers[i]);
                }
            }
    
            if (closestPlayers.Count > 0)
            {
                if (closestPlayers[index].GetComponent<Outline>() != null)
                {
                    closestPlayers[index].GetComponent<Outline>().enabled = true;
                }
                else
                {
                    closestPlayers[index].AddComponent<Outline>().enabled = true;
                }
                closestPlayers[index].GetComponent<Outline>().OutlineColor = Color.red;

                if (Input.GetKeyDown(KeyCode.T))
                {
                    if (index + 1 < closestPlayers.Count)
                    {
                        closestPlayers[index].GetComponent<Outline>().enabled = false;
                        index++;
                        closestPlayers[index].GetComponent<Outline>().enabled = true;
                        closestPlayers[index].GetComponent<Outline>().OutlineColor = Color.red;
                    }
                    else if (index + 1 >= closestPlayers.Count)
                    {
                        for (int i = 0; i < closestPlayers.Count; i++)
                        {
                            closestPlayers[index].GetComponent<Outline>().enabled = false;
                        }
                        index = 0;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.L))
                {
                    lockedOn = true;
                }
            }
            else
            {
                index = 0;
            }
        }
        else
        {
            closestPlayers[index].GetComponent<Outline>().OutlineColor = new Color(100, 0, 0);
            LockGuide.SetActive(true);
           Vector3 targetWorldPosition = closestPlayers[index].transform.position;

            // Get the camera's forward vector and the direction to the target.
            Vector3 cameraForward = cockpitCam.transform.forward;
            Vector3 toTarget = targetWorldPosition - cockpitCam.transform.position;

            // Check if the target is in front of the camera.
            float dotProduct = Vector3.Dot(cameraForward, toTarget);

            if (dotProduct > 0)
            {
                // Convert the target's world position to screen coordinates.
                Vector3 targetScreenPosition = cockpitCam.GetComponent<Camera>().WorldToScreenPoint(targetWorldPosition);

                // Define a padding value to keep some margin from the screen edges.
                float padding = 10f; // Adjust this value as needed.

                // Get the screen boundaries in pixels.
                Rect screenBounds = new Rect(padding, padding, Screen.width - 2 * padding, Screen.height - 2 * padding);

                // Clamp the targetScreenPosition to be within the screen boundaries.
                targetScreenPosition.x = Mathf.Clamp(targetScreenPosition.x, screenBounds.x, screenBounds.xMax);
                targetScreenPosition.y = Mathf.Clamp(targetScreenPosition.y, screenBounds.y, screenBounds.yMax);

                LockGuide.transform.position = targetScreenPosition;
            }
            else
            {
                // Handle the case when the target is behind the camera.
                // For example, you can move the LockGuide off-screen or deactivate it.
                
                // Move the LockGuide off-screen (assuming a fixed position).
                //Vector3 offScreenPosition = new Vector3(-1000f, -1000f, 0f);
                //LockGuide.transform.position = offScreenPosition;
                
                // OR
                
                // Deactivate the LockGuide.
                LockGuide.SetActive(false);
            }
        }
    }

    public void Respawn()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(10, 0, 0);
        stalled = false;
        shotDown = false;
        crashed = false;
        crashedText.SetActive(false);
        shotdownText.SetActive(false);
        stalledText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (!IsOwner) return;

        if (other.gameObject.GetComponent<Terrain>() != null || other.gameObject.CompareTag("Ground"))
        {
            if (!crashed)
            {
                crashed = true;
                speed = 0;
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(transform.forward * speed/2, ForceMode.Impulse);
            }
        }
    }
}