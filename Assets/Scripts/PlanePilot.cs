using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.FullSerializer;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    [Header("Pilot params")]
    public float speed = 10.0f;
    public GameObject cockpitCam;
    private Camera mainCam;
    public GameObject statusTitle;
    public GameObject stalledText;
    public GameObject shotdownText;
    public GameObject crashedText;
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
    RaycastHit hit;
    [Header("Bits of the plane")]
    public GameObject body;
    public GameObject wings;
    public GameObject tailfinLow;
    public GameObject tailfinHigh;
    private bool slowingDown;
    public GameObject curwings;
    public GameObject curtailLow;
    public GameObject curtailHigh;

    private void Start() {
        mainCam = Camera.main;
        //GameObject.Find("PlaneBuilder").GetComponent<PlaneBuilder>().GenerateWings(gameObject);
        Invoke("CheckForWings", 1);
    }
    void CheckForWings()
    {
        if (wings == null && tailfinLow == null && tailfinHigh ==  null)
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
            }
        }

        UpdateStatusText();
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