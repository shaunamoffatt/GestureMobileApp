using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;

public class CameraLongPressMove : MonoBehaviour
{
    //Camera transform
    public Transform cam;
    // To register when to drag along the floor
    public LongPressGesture longPressGesture;
    Vector3 cameraTargetLocation;
    Ray ray;
    RaycastHit hit;
    bool moveToPoint = false;
    [SerializeField] private float distanceToTarget = 20;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    private void OnEnable()
    {
        longPressGesture = Camera.main.GetComponent<LongPressGesture>();
        longPressGesture.LongPressed += LongPressGesture_Activate;
    }
    private void OnDisable()
    {
        longPressGesture.LongPressed -= LongPressGesture_Activate;
    }

    //If the screen in pressed for half a sec
    private void LongPressGesture_Activate(object sender, EventArgs e)
    {
        CheckIfTerrain();
        CheckIfSky();
    }

    void CheckIfSky()
    {
        hit = new RaycastHit();
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity))
            return;

        if (hit.collider.CompareTag("ground"))
        {
            cameraTargetLocation = hit.point;
            moveToPoint = true;
            // Move the object to the calculated location.
            StartCoroutine("MoveToRayCastPoint");
        }
    }

        void CheckIfTerrain()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();
        if (!Terrain.activeTerrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            return;

        Debug.Log("Hit Pos: " + hit.point);
        cameraTargetLocation = hit.point;
        //clamp y
        cameraTargetLocation.y = Mathf.Clamp(cameraTargetLocation.y, HandGestureUtils.minYCameraPos(), HandGestureUtils.maxYCameraPos());
        moveToPoint = true;
        // Move the object to the calculated location.
        StartCoroutine("MoveToRayCastPoint");
    }

    IEnumerator MoveToRayCastPoint()
    {
        float duration = (Vector3.Distance(cam.position, cameraTargetLocation)) / HandGestureUtils.cameraMoveSpeed();
        float delta = 0;
        Vector3 startLoc = cam.position;
        Debug.DrawLine(startLoc, cameraTargetLocation);
        while (Math.Abs(Vector3.Distance(cam.position, cameraTargetLocation)) >= distanceToTarget && moveToPoint)
        {
            Debug.DrawLine(startLoc, cameraTargetLocation);
            cam.position = Vector3.Lerp(cam.position, cameraTargetLocation, delta / duration); // smoother version
            Debug.DrawLine(cam.position, cameraTargetLocation, Color.green);
            delta += Time.deltaTime * HandGestureUtils.cameraMoveSpeed();
            yield return null;
        }
        moveToPoint = false;
    }
}
