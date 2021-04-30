using System;
using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    enum State
    {
        Holding,
        Throwing,
        Pointing,
        Idle,
        Magic
    }

    enum SpellState
    {
        Fire, Heal, Birth, Electric, None
    }

    //Remember to drag the camera to this field in the inspector
    public Transform cameraTransform;
   // public float distanceFromCamera = 10;
     public float distanceFromGround = 1;

    Vector3 flickmovement = new Vector3();

    GameObject heldItem;
    Animator anim;
    State state = State.Idle;
    SpellState currentSpell = SpellState.None;
    private GameObject spellObject;
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject electricPrefab;
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject birthPrefab;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        state = State.Idle;
    }

    void Update()
    {
        if (state != State.Pointing)
            KeepHandAboveTerrain();
    }

    internal bool HoldingObject()
    {
        if (state == State.Holding)
            return true;

        return false;
    }

    internal void DropItem(Vector3 dropPosition)
    {
        state = State.Idle;
        StartCoroutine("GrabORReleaseHeldItem", dropPosition);
    }

    internal void GrabItem(GameObject gameObject)
    {
        heldItem = gameObject;
        state = State.Holding;
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
        // move a little forward, release the item 
        //then move back
        state = State.Throwing;
        float terrainY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y + distanceFromGround;
        var hit = new RaycastHit();
        flickmovement = (transform.position + (Camera.main.transform.forward * 10));
        if (Physics.Raycast(transform.position, -Vector3.up, out hit))
        {
            // the pos goes under the ground
            if (flickmovement.y <= terrainY && hit.collider.gameObject.tag == "ground")
            {
                // Set flick movement to be the current pos (no flick will show)
                flickmovement = transform.position;
                return;
            }
        }
        StartCoroutine("ThrowAndReleaseItem", velocity);
    }

    internal void StartPointing(Vector2 screenPosition)
    {
        state = State.Pointing;
        anim.SetBool("pointing", true);
        Vector3 location = new Vector3(screenPosition.x, screenPosition.y, HandGestureUtils.handOffset().z);
        location = Camera.main.ScreenToWorldPoint(location);
        StartCoroutine(MoveToPoint(location));
    }

    internal void StopPointing()
    {
        anim.SetBool("pointing", false);
        state = State.Pointing;
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
        }
        //Play the releasing animation as The State is no longer holding
        yield return StartCoroutine(PlayGrabORReleaseAnimation());
        //Return to original pos
        //return to proper camera offset
        yield return StartCoroutine(ReturntoDefaultPosition());
        state = State.Idle;
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
        if (spellObject != null)
            Destroy(spellObject);

        spellObject = Instantiate(prefab, transform.position, Quaternion.identity);
        //Parent the speel object to the hand
        spellObject.transform.parent = gameObject.transform;
        spellObject.transform.localPosition += offset;
        foreach (ParticleSystem p in spellObject.GetComponentsInChildren<ParticleSystem>())
            p.Play();
    }

    IEnumerator MoveToPoint(Vector3 location)
    {
        //Move to position of the item (to pick up or drop)
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
        if (state == State.Holding)
            anim.SetBool("holding", true);
        else
            anim.SetBool("holding", false);
        //Then pick up item-( Different Kinds of pickup Methods depending on the Gameobject)
        if (state != State.Throwing)
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
                    if (state == State.Holding)
                        heldItem.GetComponent<NPCController>().PickUp();
                    else
                        heldItem.GetComponent<NPCController>().Drop();
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
