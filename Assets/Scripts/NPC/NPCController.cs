using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    [SerializeField] private GameObject ragdoll;
    [SerializeField] private GameObject model;
    [SerializeField] private NavMeshAgent agent;
    // For when the NPC is held
    [SerializeField] public Transform hand;
    // RigidBody- set to be the first child joint of the ragdoll
    private Rigidbody rb;
    // deathParticle
    [SerializeField] GameObject deathParticle;

    private int health = 100;

    enum State
    {
        Held,
        Normal,
        Flung

    }
    private State state = State.Normal;

    private void Awake()
    {
        ragdoll.gameObject.SetActive(false);
        SetRigidBody();
        if (deathParticle == null)
        {
            Debug.LogError("NPC has no deathParticle");
        }
        EnableNavMeshAgent();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        model.transform.position = transform.position;
        ragdoll.transform.position = transform.position;

        switch (state)
        {
            case State.Normal:
                {
                    if (!agent.isOnNavMesh)
                    {
                        EnableNavMeshAgent();
                    }
                    else if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathComplete || agent.pathStatus == NavMeshPathStatus.PathPartial)
                    {
                        ChooseRandomPosition();
                    }
                    break;
                }
            case State.Flung:
                {
                    // transform.rotation = Quaternion.LookRotation(parentRB.velocity);
                    // SEt the position to follow the hips rigid body

                    //float terrainY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y + 1;
                    //if (rb.transform.position.y <= 10)
                    //{
                    // This should happen after the first rigid body gets some velocty
                    CheckIfStopped();
                    // }
                    break;
                }
            case State.Held:
                {
                    //Keep the ragdolls rb with the held model
                    // ragdoll.transform.position = transform.position;
                    rb.transform.position = transform.position;
                    break;
                }
        }

        CheckIfDead();
    }

    [SerializeField] float randomRange = 1;
    private void ChooseRandomPosition()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * randomRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, randomRange, 1);
        agent.SetDestination(hit.position);
    }

    private void CheckIfDead()
    {
        if (health <= 0)
        {
            //Destroy the NPC
            StartCoroutine(KillEnemy());
            // TODO Add Score

        }
    }

    private void CheckIfStopped()
    {
        // if the hips have stopped moving
        if (rb.velocity.magnitude < 0.01f)// and colliding with the ground{
        {
            state = State.Normal;
            //SEt the position to be where the rigidbody is
            transform.position = rb.position;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ToggleRagDoll(false);

        }
    }

    public void PickUp()
    {
        //Stop the navmesh agent so the npc stops walking around
        agent.enabled = false;
        // Set the position to the hands postion
        transform.position = hand.position;
        // Set state to Held
        state = State.Held;
        //Set Parent to the hand
        this.transform.parent = GameObject.Find("hand").transform;
        //set the new local pos
        transform.localPosition = HandGestureUtils.holdingLocalPostition();
    }

    public void Drop()
    {
        //Start the navmesh agent so the npc starts walking around
        EnableNavMeshAgent();
        // Set the state to normal
        state = State.Normal;
        // Unparent from the hand
        transform.parent = null;
    }

    Vector3 velocity;
    public void Throw(Vector3 velocity)
    {
        // Act as a ragDoll
        ToggleRagDoll(true);
        // Add Flick Force
        if (rb != null)
        {
            this.velocity = velocity;
            AddForceToRagDoll();
        }
        Debug.Log("VELOCITY**********************" + velocity);
        //Unparent from hand
        transform.parent = null;
        //Set the gravity to true so it falls
        // rb.useGravity = true;

    }

    [ContextMenu("ToggleRagDoll")]
    private void ToggleRagDoll(bool activateRagDoll)
    {
        if (activateRagDoll)
        {
            CopyTransformData(model.transform, ragdoll.transform, agent.velocity);
            ragdoll.gameObject.SetActive(true);
            model.gameObject.SetActive(false);
            agent.enabled = false;
        }
        else
        {
            //Swith to the model and disable the ragdoll
            ragdoll.gameObject.SetActive(false);
            model.gameObject.SetActive(true);
            EnableNavMeshAgent();
        }
    }

    private void EnableNavMeshAgent()
    {
        float terrainY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y + 1;
        agent.enabled = true;
        agent.autoRepath = true;
        if (!agent.isOnNavMesh)
        {

            agent.transform.position = transform.position + new Vector3(0, terrainY, 0);
            // Dont remember reasoning... nav mesh agent seem to work oddly-
            // I think something to do with getting it on the Navmesh
            agent.enabled = false;
            agent.enabled = true;
        }
    }

    private void CopyTransformData(Transform sourceTransform, Transform destinationTransform, Vector3 velocity)
    {
        if (sourceTransform.childCount != destinationTransform.childCount)
        {
            Debug.LogWarning("Error, invalid transform copy for model and ragdoll");
            return;
        }

        for (int i = 0; i < sourceTransform.childCount; i++)
        {
            var source = sourceTransform.GetChild(i);
            var destination = destinationTransform.GetChild(i);
            destination.position = source.position;
            destination.rotation = source.rotation;
            var _rb = destination.GetComponent<Rigidbody>();
            if (_rb != null)
            {
                if (rb == null)
                {
                    //First time to set the rigid body
                    rb = _rb;// will controll where to throw the NPC
                }
                _rb.velocity = velocity;
            }
            //Recursive...
            CopyTransformData(source, destinationTransform, velocity);
        }
    }

    //Sets a reference rigid body for where the model should be when activated
    void SetRigidBody()
    {
        Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            if (rb == null && rigidbody.tag == "rb")
            {
                //First time to set the rigid body
                rb = rigidbody;// will controll where to throw the NPC
                Debug.Log("RigidBody set");
                return;
            }
        }
    }


    // Adds a force to all the limbs of the ragdoll
    void AddForceToRagDoll()
    {
        Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            StartCoroutine(ApplyForce(rigidbody));
        }

    }


    IEnumerator ApplyForce(Rigidbody body)
    {
        float time = 0.5f;
        // Apply the force smoothly over time!
        // Check if the rigid body we apply force to is still alive.
        // In a real game situation it might have been hit by and explosion and was destroyed.
        while (time > 0 && body != null)
        {
            body.AddForce(velocity, ForceMode.Impulse);

            // Yields until the next fixed update frame.
            // Forces always need to be applied at fixed frame rate!               
            yield return new WaitForFixedUpdate();
            time -= Time.deltaTime;
        }
        //Using the hips- check the rb is in motion and change state
        if (body == rb && rb.velocity.magnitude > 0.1f)
        {
            state = State.Flung;
        }
    }

    private void Damage(int amount)
    {
        health -= amount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Terrain>() != null)
        {

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        state = State.Flung;
        ToggleRagDoll(true);
    }

    IEnumerator KillEnemy()
    {
        //Play death particles
        Instantiate(deathParticle, transform);
        //Start RagDollEffect
        ToggleRagDoll(true);
        //Wait 5 secs before destroying
        yield return new WaitForSecondsRealtime(5f);
        Destroy(gameObject);
    }
}
