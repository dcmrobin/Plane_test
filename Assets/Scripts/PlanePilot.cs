using UnityEngine;
using Unity.Netcode;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    public float speed = 10.0f;
    [Header("Gun params")]
    public int gunDamage = 2;
    public LayerMask targetMask;
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

    /*private void Start() {
        if (IsOwner)
        {
            
    
            
        }
    }*/
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
        Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);
        Camera.main.transform.LookAt(transform.position + transform.forward * 30.0f);

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

            if (Input.GetKey(KeyCode.Space))
            {
                slowingDown = true;
                speed -= 1;
                if (speed <= 0)
                {
                    speed = 0;
                }
                GetComponent<Rigidbody>().isKinematic = false;
            }
            else
            {
                slowingDown = false;
                GetComponent<Rigidbody>().isKinematic = true;
            }
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
    }

    public void Fire()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetMask))
        {
            hit.collider.GetComponent<Damageable>().GetDamagedServerRpc(gunDamage);
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