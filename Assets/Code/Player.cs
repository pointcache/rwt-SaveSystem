using UnityEngine;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    Rigidbody rb;
    public float accel = 2f;
    public float maxspeed = 1f;
    public float maxVelocityChange = 2f;

    void OnEnable()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        float hor = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 targetVelocity = new Vector3(hor * accel, 0f, vert * accel);
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);


    }
}
