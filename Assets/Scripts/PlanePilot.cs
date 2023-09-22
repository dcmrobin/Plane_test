using UnityEngine;
using Unity.Netcode;

public class PlanePilot : NetworkBehaviour
{
    //public bool usingTerrain = true;
    public float speed = 10.0f;
    public bool crashed;
    public bool stalled;

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
            transform.Rotate(Input.GetAxis("Vertical") * 2, 0.0f, -Input.GetAxis("Horizontal") * 2);
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
