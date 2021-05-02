using System;
using System.Collections;
using UnityEngine;

public enum HandState
{
    Holding,
    Throwing,
    Dropping,
    Pointing,
    Idle,
    Magic
}

public class HandController : MonoBehaviour
{
    enum SpellState
    {
        Fire, Heal, Birth, Electric, None
    }

    //Remember to drag the camera to this field in the inspector
    public Transform cameraTransform;
    // public float distanceFromCamera = 10;
    public float distanceFromGround = 1;

    Vector3 flickmovement = new Vector3();

    private GameObject heldItem;
    Animator anim;
    HandState state = HandState.Idle;
    SpellState currentSpell = SpellState.None;
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject electricPrefab;
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject birthPrefab;

    public void setState(HandState state)
    {
        this.state = state;
    }

    public HandState getState()
    {
        return state;
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        setState(HandState.Idle);
    }

    void Update()
    {
        if (state != HandState.Pointing)
            KeepHandAboveTerrain();
    }

    internal void DropItem(Vector3 dropPosition)
    {
        setState(HandState.Dropping);
        StartCoroutine("GrabORReleaseHeldItem", dropPosition);
    }

    internal void GrabItem(GameObject gameObject)
    {
        setState(HandState.Holding);
        heldItem = gameObject;
        StartCoroutine("GrabORReleaseHeldItem", heldItem.transform.position);
    }

    IEnumerator GrabORReleaseHeldItem(Vector3 location)
    {
        //Play the grabbing or releasing animation and pickup or drop item
        yield return StartCoroutine(MoveToPoint(location));
        //Play the grabbing or releasing animation and pickup or drop item
        yield return StartCoroutine(PlayGrabORReleaseAnimation());
        //return to proper camera offset
        yield return StartCoroutine(ReturntoDefaultPosition());
        yield break;
    }

    internal void ThrowItem(Vector3 velocity)
    {
        if (heldItem != null)
        {
            // move a little forward, release the item 
            //then move back
            setState(HandState.Throwing);
            float terrainY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y + distanceFromGround;
            var hit = new RaycastHit();
            flickmovement = (transform.position + (Camera.main.transform.forward * 2));
            //Check if the hand goies under the terrain
            if (Physics.Raycast(transform.position, -Vector3.up, out hit))
            {
                // the pos goes under the ground
                if (flickmovement.y <= terrainY && hit.collider.gameObject.tag == "ground")
                {
                    Vector3 p = new Vector3(transform.position.x, transform.position.y - terrainY, transform.position.z);
                    //  Set flick movement to be the current pos (no flick will show) - terrains y pos
                    flickmovement = p;
                    return;
                }
            }
            StartCoroutine("ThrowAndReleaseItem", velocity);
        }
        
    }

    internal void StartPointing(Vector2 screenPosition)
    {
        setState(HandState.Pointing);
        anim.SetBool("pointing", true);
        Vector3 location = new Vector3(screenPosition.x, screenPosition.y, HandGestureUtils.handOffset().z);
        location = Camera.main.ScreenToWorldPoint(location);
        StartCoroutine(MoveToPoint(location));
    }

    internal void StopPointing()
    {
        anim.SetBool("pointing", false);
        StartCoroutine(ReturntoDefaultPosition());
    }

    internal void PointMove(Vector2 screenPosition)
    {
        Vector3 location = new Vector3(screenPosition.x, screenPosition.y, HandGestureUtils.handOffset().z);
        location = Camera.main.ScreenToWorldPoint(location);
        //transform.position = location;
        StartCoroutine(MoveToPoint(location));
    }

    IEnumerator ThrowAndReleaseItem(Vector3 velocity)
    {
        yield return StartCoroutine(MoveToPoint(flickmovement));
        //Throw item
        switch (heldItem.layer)
        {
            case 9://person
                {
                    heldItem.GetComponent<NPCController>().Throw(velocity);
                    break;
                }
            case 11://fireball
                {
                    heldItem.GetComponent<ThrowableParticleController>().Throw(velocity);
                    break;
                }
        }
       
        //Play the releasing animation as The State is no longer holding
        yield return StartCoroutine(PlayGrabORReleaseAnimation());
        //Return to original pos
        //return to proper camera offset
        yield return StartCoroutine(ReturntoDefaultPosition());
        // Finally reset to Idle
        setState(HandState.Idle);
        yield break;
    }

    internal void LoadFireSpell()
    {
        Debug.Log("FIRE SPELL LOADING.........");
        LoadSpell(fireBallPrefab, Vector3.zero);
    }

    internal void LoadHealSpell()
    {
        Debug.Log("HEAL SPELL LOADING.........");
        LoadSpell(healPrefab, Vector3.zero);
    }

    internal void LoadBirthSpell()
    {
        Debug.Log("BIRTH SPELL LOADING.........");
        LoadSpell(birthPrefab, new Vector3(0, 0, -2));
    }

    internal void LoadElectricSpell()
    {
        Debug.Log("ELECTRIC SPELL LOADING.........");
        LoadSpell(electricPrefab, new Vector3(0, 0, -2));

    }

    private void LoadSpell(GameObject prefab, Vector3 offset)
    {
        // State is Magic
        setState(HandState.Magic);
        // if the layer is less then 10 its not an object
        if (heldItem != null)
            Destroy(heldItem);

        heldItem = Instantiate(prefab, transform.position, Quaternion.identity);
        //Parent the speel object to the hand
        heldItem.transform.parent = gameObject.transform;
        heldItem.transform.localPosition += offset;
        foreach (ParticleSystem p in heldItem.GetComponentsInChildren<ParticleSystem>())
            p.Play();
    }

    IEnumerator MoveToPoint(Vector3 location)
    {
        //Move to position 
        float duration = (Vector3.Distance(transform.position, location)) / HandGestureUtils.cameraMoveSpeed();
        float delta = 0;
        float distanceX = Math.Abs(transform.position.x - location.x);
        float distanceZ = Math.Abs(transform.position.z - location.z);
        while (distanceX >= 0.1f && distanceZ >= 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, location, delta / duration);
            delta += Time.deltaTime * HandGestureUtils.cameraMoveSpeed();
            distanceX = Math.Abs(transform.position.x - location.x);
            distanceZ = Math.Abs(transform.position.z - location.z);
            yield return null;
        }
    }

    IEnumerator PlayGrabORReleaseAnimation()
    {
        //Play the grabbing animation and pick up item
        if (state == HandState.Holding)
            anim.SetBool("holding", true);
        else
            anim.SetBool("holding", false);
        //Then pick up item-( Different Kinds of pickup Methods depending on the Gameobject)
        if (state != HandState.Throwing)
            DropOrPickUp();
        //Wait for the animation to end
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);
    }

    //Picks up or places the item
    void DropOrPickUp()
    {
            switch (heldItem.layer)
            {
                case 9://person
                    {
                        //Then pick up person if the state is in Holding
                        if (state == HandState.Holding)
                            heldItem.GetComponent<NPCController>().PickUp();
                        else
                        {
                            heldItem.GetComponent<NPCController>().Drop();
                            // Finally reset to Idle
                            setState(HandState.Idle);
                            heldItem = null;
                        }
                        break;
                    }
            }
    }

    IEnumerator ReturntoDefaultPosition()
    {
        //Finally return to position
        float duration = (Vector3.Distance(transform.localPosition, HandGestureUtils.handOffset())) / HandGestureUtils.cameraMoveSpeed();
        float delta = 0;
        while (Math.Abs(Vector3.Distance(transform.localPosition, HandGestureUtils.handOffset())) >= 0.1f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, HandGestureUtils.handOffset(), delta / duration);
            delta += Time.deltaTime * HandGestureUtils.cameraMoveSpeed();
            yield return null;
        }
    }

    private void KeepHandAboveTerrain()
    {
        float terrainY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y + distanceFromGround;
        var hit = new RaycastHit();
        if (Physics.Raycast(transform.position, -Vector3.up, out hit))
        {
            if (transform.position.y <= terrainY && hit.collider.gameObject.tag == "ground")
            {
                Vector3 CurrentPos = transform.position;
                CurrentPos.y = terrainY;
                transform.position = CurrentPos;
                return;
            }
            // Check if high enough above ground-> set to default pos
            if (Vector3.Distance(transform.position, hit.point) >= 3f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, HandGestureUtils.handOffset(), 5 * Time.deltaTime);
            }
        }
    }
}
