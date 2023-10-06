using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissile : MonoBehaviour
{
    public Transform target;
    public float speed = 50;
    public float rotationSpeed = 100;
    private Rigidbody rb;


    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        Vector3 direction = (target.position - transform.position).normalized;

        // Rotate the missile towards the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rb.rotation = Quaternion.Slerp(rb.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Move the missile forward
        rb.velocity = transform.forward * speed;
    }
}
