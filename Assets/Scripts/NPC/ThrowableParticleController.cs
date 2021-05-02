using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableParticleController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float particleForce = 10;
    private bool thrown = false;
    [SerializeField] float lifeTime = 7;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        thrown = false;
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
        rb.isKinematic = false;
        rb.AddForce(velocity* particleForce, ForceMode.Impulse);
        thrown = true;
        // Play exit
    }

    private void Update()
    {
        if (thrown)
        {
            if(timer >= lifeTime)
            {
                ParticleSystem[] particles = GetComponents<ParticleSystem>();
                //play the end of the particle
                foreach(ParticleSystem p in particles)
                {
                    p.Stop(true);
                    Destroy(p);
                }
                Destroy(gameObject);
            }
            timer += Time.deltaTime;
        }
    }
}
