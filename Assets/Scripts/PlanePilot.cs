using UnityEngine;
using Unity.Netcode;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    public float speed = 10.0f;
    public float gunDamage = 2.0f;
    public bool crashed;
    public bool stalled;
    public LayerMask targetMask;
    RaycastHit hit;
    public GameObject body;
    public GameObject wings;
    public GameObject tailfinLow;
    public GameObject tailfinHigh;

    private void FixedUpdate() {
        if (!IsOwner) return;

        Vector3 moveCamTo = transform.position - transform.forward * 10.0f + Vector3.up * 5.0f;
        float bias = 0.96f;
        Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);
        Camera.main.transform.LookAt(transform.position + transform.forward * 30.0f);

        if (!crashed)
        {
            if (!stalled)
            {
                transform.position += transform.forward * Time.deltaTime * speed;
            }
            speed -= transform.forward.y * Time.deltaTime * 50.0f;
            //transform.Rotate(Input.GetAxis("Vertical") * 2, 0.0f, -Input.GetAxis("Horizontal") * 2);
            if (tailfinLow != null)
            {
                transform.Rotate(Input.GetAxis("Vertical") * 2, 0.0f, 0.0f);
            }
            if (wings != null)
            {
                transform.Rotate(0.0f, 0.0f, -Input.GetAxis("Horizontal") * 2);
            }
        }

        if (speed <= 0.0f)
        {
            speed = 0.0f;
            stalled = true;
            //speed = 35.0f;//not realistic... the engine should stall
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed/2, ForceMode.Impulse);
        }
        else
        {
            stalled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Update() {
        if (body.GetComponent<Damageable>().health <= 0)
        {
            crashed = true;
            speed = 0;
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().AddForce(transform.forward * speed/2, ForceMode.Impulse);
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