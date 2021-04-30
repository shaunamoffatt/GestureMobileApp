using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]

public class EnemyController : MonoBehaviour
{
    enum State
    {
        Held,
        Wander,
        Flung
    }

    [SerializeField] GameObject deathParticle;
    [SerializeField] Transform handPosition;
    private State state = State.Wander;

    public float lookRadius = 10f;
    Transform target;
    NavMeshAgent agent;

    [SerializeField] public float patrolSpeed = 2f;
    [SerializeField] float rotateSpeed = 5f;

    // used for patrolling
    float patrolStartTime = 0;
    private Vector3 randomSpot;
    public float randomRange = 10f;

    //set wait times for patroling
    private float waitTime;
    public float startWaitTime = 1f;

    Rigidbody rb;
    CapsuleCollider capsuleCollider;

    private const int DEADLAYER = 16;

    //Control the different sound of ememies
    //SoundManager.Sound soundDie, soundAlert;
    // used to briefly stop the enemy chasing the player onCollision
    bool collided = false;

    // Start is called before the first frame update
    void Start()
    {
        
        InitializeSound();
        randomSpot = ChooseRandomSpot();
        //target.position = ChooseRandomSpot();
        //init deathparticle
        if (deathParticle == null)
        {
            Debug.LogError("EnemyControlller has no deathParticle");
        }
        //Play death particles once when spawing
        deathParticle.SetActive(true);
        rb = GetComponent<Rigidbody>();
        ResetNPC();
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //If they are dead return
        if (gameObject.layer == DEADLAYER)
            return;

        switch (state)
        {
            case State.Wander:
                {
                    Patrol();
                    break;
                }
            case State.Held:
                {
                    //transform.position = handPosition.position;
                    // Check if hit ground?
                    break;
                }
            case State.Flung:
                {

                    //Check if on the ground stable
                    CheckIfStopped();
                    break;
                }
        }
    }

    int stoppedRBCount = 0;
   
    private void CheckIfStopped()
    {
            stoppedRBCount = 0;
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
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
                state = State.Wander;
                ResetNPC();
            }
            //Check if flung into water 
            //then kill
    }

    private void ResetNPC()
    {
        //Set the ragdoll to false
        //Reset Rotation
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        waitTime = startWaitTime;
        this.velocity = Vector3.zero;
        //Turn off RagDoll
        TurnOffRagDoll();
        //Reset Rigid Body
        InitializeRigidBody();

        //Reset Rotation
        EnableNavMeshAgent();
    }

    private void EnableNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
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

    Vector3 ChooseRandomSpot()
    {
        float randomSpotx = transform.position.x + Random.Range(-randomRange, +randomRange);
        float randomSpotz = transform.position.z + Random.Range(-randomRange, +randomRange);

        return new Vector3(randomSpotx, 0, randomSpotz);
    }

    public void PickUp()
    {
        //Turn off Gravity
        rb.useGravity = false;
        capsuleCollider.enabled = false;
        //Turn off agent
        agent.enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        //Set the position to the hands postion
        transform.position = handPosition.position;

        state = State.Held;
        this.transform.parent = GameObject.Find("hand").transform;
        //set the new local pos
        transform.localPosition = HandGestureUtils.holdingLocalPostition();
    }

    //Rest
    public void Drop()
    {
        //Turn on Gravity
        rb.useGravity = true;
        capsuleCollider.enabled = true;
        // Turn back on NavMesh Agent
        EnableNavMeshAgent();
        //Set state to Wander
        state = State.Wander;
        // Unparent from the hand
        transform.parent = null;
    }

    Vector3 velocity;
    public void Throw(Vector3 velocity)
    {
        // Turn off Kinematic to allow for Forces
        // Act as a ragDoll
        //Stop  Animations
        GetComponentInChildren<Animator>().enabled = false;
        // make sure agent disabled
        TurnOnRagDoll();
        agent.enabled = false;

        // Unparent from the hand
        this.velocity = velocity;
        // TurnOnRagDoll(true);
        AddForceToRagDoll();
        Debug.Log("VELOCITY**********************" + velocity);
        //Wait 3 secs to change state- this then checks if stopped
        StartCoroutine(WaitToChangeState());  
    }


    IEnumerator WaitToChangeState()
    {
        yield return new WaitForSeconds(3);
        state = State.Flung;
    }

    void AddForceToRagDoll()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            // rigidbody.isKinematic = false;
            rigidbody.AddForce(velocity, ForceMode.Impulse);
        }
        //rb.AddForce(velocity, ForceMode.Impulse);
        //Unparent from hand
        transform.parent = null;
    }

    void InitializeRigidBody()
    {
        //Init Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        this.velocity = Vector3.zero;
    }

    //TODO add enemy sounds
    void InitializeSound()
    {

    }

    void ChaseEnemy()
    {
        //Chase player
        agent.destination = target.transform.position;
        //Play Alert sound
        //SoundManager.PlaySound(soundAlert, transform.position);
        //FaceTarget
        FaceTarget(target.position);
    }

    void Patrol()
    {
        if (agent.isOnNavMesh && !collided)
        {
            ///move to a random position
            agent.SetDestination(randomSpot);
            FaceTarget(randomSpot);
            agent.speed = patrolSpeed;

            if (!agent.pathPending && agent.remainingDistance < 0.1f || patrolStartTime < 5f)
            {
                patrolStartTime += Time.deltaTime;
                if (waitTime <= 0)
                {
                    randomSpot = ChooseRandomSpot();
                    waitTime = startWaitTime;
                    patrolStartTime = 0f;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        //Using the players tag 17 to stop chase briefly
        if (collision.gameObject.layer == 17)
        {
            collided = true;
            Debug.Log("Collided with player");
            StartCoroutine(Rotate());
        }
    }

    //Rotate 360degrees
    IEnumerator Rotate()
    {
        float startRotation = transform.eulerAngles.y;
        float endRotation = startRotation + 360.0f;
        float t = 0.0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotation, t / 1f) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            yield return null;

        }
        yield return new WaitForSeconds(2);
        collided = false;

    }

    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        //rotation to look at target
        Quaternion lookAtRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAtRotation, Time.deltaTime * rotateSpeed);
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("hit the NPC");
        //Play Enemy die sound
        //SoundManager.PlaySound(soundDie, transform.position);

        //Add an implulse force and disable NavMesh agent
        agent.enabled = false;
        rb.AddForce(-transform.forward * 20, ForceMode.Impulse);

        //StartCoroutine(KillEnemy());
    }

    IEnumerator KillEnemy()
    {
        //Play death particles
        deathParticle.SetActive(true);
        //Change Layer so that it cant hurt the  player
        //TODO Set the layers up in const game manager DEADPLAYER == 16
        // SetLayerRecursively(gameObject, DEADLAYER);
        Debug.Log("LAyer : " + gameObject.layer);
        //Stop  Animations
        GetComponentInChildren<Animator>().enabled = false;
        //Start RagDollEffect
        TurnOnRagDoll();
        //Wait 5 secs before destroying
        yield return new WaitForSecondsRealtime(5f);
        Destroy(gameObject);
    }

    // Set the child rigibody states to true and the parents state to false (or vice versa)
    // Set to false to make the Children Kinematic
    void TurnOnRagDoll()
    {
        //Turn of the main collider
        capsuleCollider.enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void TurnOffRagDoll()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.angularVelocity = Vector3.zero;

        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        //Turn on Animation
        GetComponentInChildren<Animator>().enabled = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        capsuleCollider.enabled = true;
    }

    //Change all the child objects layers to Dead which is 16
    public static void SetLayerRecursively(GameObject g, int layerNumber)
    {
        if (null == g)
            return;
        g.layer = layerNumber;
        foreach (Transform child in g.transform)
        {
            if (null == child)
                continue;
            child.gameObject.layer = layerNumber;
            SetLayerRecursively(child.gameObject, layerNumber);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.DrawWireSphere(randomSpot, 3);
    }
}
