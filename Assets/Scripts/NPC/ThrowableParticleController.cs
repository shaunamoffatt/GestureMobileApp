using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableParticleController : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 velocity)
    {
        // Add Flick Force
        Debug.Log("VELOCITY**********************" + velocity);
        //Unparent from hand
        transform.parent = null;
        //Set the gravity to true so it falls
        rb.useGravity = true;
        rb.AddForce(velocity, ForceMode.Impulse);
        // Play exit
    }
}
