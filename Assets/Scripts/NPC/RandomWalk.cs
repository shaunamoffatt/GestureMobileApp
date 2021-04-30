using UnityEngine;
using UnityEngine.AI;

public class RandomWalk : MonoBehaviour
{
    private NavMeshAgent agent;
    public float randomRange = 5f;

    // Start is called before the first frame update
    void Start()
    {
        EnableNavMeshAgent();
    }

    // Update is called once per frame
    void Update()
    {
        //if (agent.enabled == false) 
           // return;

       // if (agent.hasPath == false || agent.remainingDistance < 0.1f)
      ///  {
      //      ChooseRandomPosition();
       // }
    }

    private void ChooseRandomPosition()
    {
        float randomSpotx = transform.position.x + Random.Range(-randomRange, +randomRange);
        float randomSpotz = transform.position.z + Random.Range(-randomRange, +randomRange);
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(new Vector3(randomSpotx, 0, randomSpotz));
        }
        else
        {
            EnableNavMeshAgent();
        }
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
}
