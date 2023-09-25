using UnityEngine;
using Unity.Netcode;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    public float speed = 10.0f;
    public int gunDamage = 2;
    public bool crashed;
    public bool stalled;
    public bool shotDown;
    public LayerMask targetMask;
    RaycastHit hit;
    [Header("Bits of the plane")]
    public GameObject body;
    public GameObject wings;
    public GameObject tailfinLow;
    public GameObject tailfinHigh;
    private bool slowingDown;
    /*[Header("Prefabs")]
    [SerializeField] private GameObject wingsPrefab;
    [SerializeField] private GameObject tailfinLowPrefab;
    [SerializeField] private GameObject tailfinHighPrefab;

    private void Start() {
        SpawnWingsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnWingsServerRpc()
    {
        GameObject spawnedwings = Instantiate(wingsPrefab, Vector3.zero, Quaternion.identity);
        spawnedwings.GetComponent<NetworkObject>().Spawn();
        spawnedwings.transform.SetParent(transform);
        spawnedwings.transform.position = wings.transform.position;
        spawnedwings.transform.rotation = wings.transform.rotation;
        Destroy(wings);
        wings = spawnedwings;

        GameObject spawnedtailfinLow = Instantiate(tailfinLowPrefab, Vector3.zero, Quaternion.identity);
        spawnedtailfinLow.GetComponent<NetworkObject>().Spawn();
        spawnedtailfinLow.transform.SetParent(transform);
        spawnedtailfinLow.transform.position = tailfinLow.transform.position;
        spawnedtailfinLow.transform.rotation = tailfinLow.transform.rotation;
        Destroy(tailfinLow);
        tailfinLow = spawnedtailfinLow;

        GameObject spawnedtailfinHigh = Instantiate(tailfinHighPrefab, Vector3.zero, Quaternion.identity);
        spawnedtailfinHigh.GetComponent<NetworkObject>().Spawn();
        spawnedtailfinHigh.transform.SetParent(transform);
        spawnedtailfinHigh.transform.position = tailfinHigh.transform.position;
        spawnedtailfinHigh.transform.rotation = tailfinHigh.transform.rotation;
        Destroy(tailfinHigh);
        tailfinHigh = spawnedtailfinHigh;
    }*/
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