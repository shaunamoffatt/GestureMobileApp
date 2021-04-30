using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

// Adapted from Tutorial https://www.youtube.com/watch?v=cHVZ0SYIHkI&ab_channel=Holistic3d
// and https://www.youtube.com/watch?v=xlwuGKTyJBs&ab_channel=Holistic3d
public class SwipeTrail : MonoBehaviour
{
    Plane objPlane;
    float rayDistance = 10;
    GameObject thisTrail;
    public GameObject trailPrefab;
    private Vector3 startPos;
    private float startTime;

    //public ScreenTransformGesture transformGesture;
    void Start()
    {
        objPlane = new Plane(Camera.main.transform.forward * (-1), transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
           // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 location = new Vector3(Input.mousePosition.x, Input.mousePosition.y, HandGestureUtils.handOffset().z);
            location = Camera.main.ScreenToWorldPoint(location);
            // objPlane.Raycast(ray, out rayDistance);
            //startPos = ray.GetPoint(rayDistance);

            thisTrail = Instantiate(trailPrefab,
             location, Quaternion.identity);

            //thisTrail.transform.parent = gameObject.transform;

        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            objPlane.Raycast(ray, out rayDistance);
            //thisTrail.transform.position = ray.GetPoint(rayDistance);
            Vector3 location = new Vector3(Input.mousePosition.x, Input.mousePosition.y, HandGestureUtils.handOffset().z);
            location = Camera.main.ScreenToWorldPoint(location);
            thisTrail.transform.position = location;
            startPos = thisTrail.transform.position;

        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (Vector3.Distance(thisTrail.transform.position, startPos) < 0.5f)
                Destroy(thisTrail);
        }
    }
}
