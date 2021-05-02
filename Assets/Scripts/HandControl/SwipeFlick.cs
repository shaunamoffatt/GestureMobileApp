using System;
using TouchScript.Gestures;
using UnityEngine;

public class SwipeFlick : MonoBehaviour
{
    public FlickGesture flickGesture;
    public HandController hand;
    [SerializeField] private float flickForce = 35;
    [SerializeField] private float launchAngle = 60;
    Vector3 velocity;
    private void Start()
    {
        if (hand == null)
            hand = GameObject.Find("hand").GetComponent<HandController>();
    }

    private void OnEnable()
    {
        flickGesture = Camera.main.GetComponent<FlickGesture>();
        flickGesture.Flicked += FlickGesture_Activate;
    }

    private void OnDisable()
    {
        flickGesture.Flicked -= FlickGesture_Activate;
    }

    private void FlickGesture_Activate(object sender, EventArgs e)
    {
        Debug.Log("distance******************************** " + distanceFromCamera);
        Vector3 velocity = Launch();
        Debug.Log("VELOCITY******************************** " + velocity);
        hand.ThrowItem(velocity);
    }

    float distanceFromCamera;
    //Adapted from tutorial https://vilbeyli.github.io/Projectile-Motion-Tutorial-for-Arrows-and-Missiles-in-Unity3D/
    Vector3 Launch()
    {
        // Distance from Camera depends on the flick
        if (flickGesture.ScreenFlickTime != 0)
            distanceFromCamera = flickForce / flickGesture.ScreenFlickTime;
        else
            distanceFromCamera = 2;
        
        // think of it as top-down view of vectors: 
        Debug.Log("distanceFromCamera******************************** " + distanceFromCamera);
        // we don't care about the y-component(height) of the initial and target position.
        Vector3 currentXZpos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        // Target pos- current pos + X and Z of the Camera transform forward * distance from camera
        Vector3 camDir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
        camDir.Normalize();
        camDir *= distanceFromCamera * 2;
        Vector3 targetPosition = currentXZpos - camDir;

        // Get distance between current and target
        float R = Vector3.Distance(currentXZpos, targetPosition);
        Debug.Log("distance******************************** " + R);
        float G = Physics.gravity.y;
        //float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float tanAlpha = Mathf.Tan(launchAngle / 2 * Mathf.Deg2Rad);
        float H = targetPosition.y - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;
        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, -Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);
        // launch the object by setting its initial velocity and flipping its state
        return globalVelocity;
    }
}
