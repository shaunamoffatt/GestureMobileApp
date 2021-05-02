using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    //used to count 
    int stoppedRBCount = 0;
    private int health = 100;
    // test
    private Rigidbody parentRB;

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
        parentRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        model.transform.position = transform.position;
        ragdoll.transform.position = transform.position;

        switch (state)
        {
            case State.Normal:
                {
                    if (agent.hasPath == false || agent.remainingDistance < 0.1f)
                    {
                        ChooseRandomPosition();
                    }
                    break;
                }
            case State.Flung:
                {
                    transform.rotation = Quaternion.LookRotation(parentRB.velocity);
                    // Check if on the ground stable
                    CheckIfStopped();
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

    private float randomRange = 2f;
    private void ChooseRandomPosition()
    {
        float randomSpotx = transform.position.x + UnityEngine.Random.Range(-randomRange, +randomRange);
        float randomSpotz = transform.position.z + UnityEngine.Random.Range(-randomRange, +randomRange);
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(new Vector3(randomSpotx, 0, randomSpotz));
        }
        else
        {
            EnableNavMeshAgent();
        }
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
        stoppedRBCount = 0;
        Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            if (rb.velocity.magnitude < 0.1f)// and colliding with the ground{
            {
                stoppedRBCount++;
            }
        }
        // If more than 4 rigidbodies (ie one of the limbs) have stopped- rest the tika man
        if (stoppedRBCount >= 4)
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
        // Dontuse gravity for the ragdoll while being flung 
        //UseGravityRagdoll(false);
        // Add Flick Force
        if (rb != null)
        {
            this.velocity = velocity;

            //parentRB.velocity = velocity;
            //rb.AddForce(velocity, ForceMode.Impulse);
            //rb.AddForce(Vector3.up * 30, ForceMode.Impulse);
            AddForceToRagDoll();
        }

        Debug.Log("VELOCITY**********************" + velocity);
        //Unparent from hand
        transform.parent = null;
        //Set the gravity to true so it falls
        // rb.useGravity = true;
        state = State.Flung;
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
        agent.enabled = true;
        if (!agent.isOnNavMesh)
        {
            agent.transform.position = transform.position;
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
            //rigidbody.useGravity = true;
            //rigidbody.AddForce(velocity, ForceMode.Impulse);
            // rigidbody.useGravity = useGravity;
            //rigidbody.velocity = velocity;
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
    }

    private void Damage(int amount)
    {
        health -= amount;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Terrain>() != null)
        {
           // UseGravityRagdoll(true);
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
