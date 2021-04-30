using System;
using TouchScript.Gestures;
using UnityEngine;

public class SwipeFlick : MonoBehaviour
{
    public FlickGesture flickGesture;
    public HandController hand;
    [SerializeField] private float flickForce = 35;

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
        if (!hand.HoldingObject())
            return;

        float speed = flickForce / flickGesture.ScreenFlickTime;
        Debug.Log("SPEED******************************** " + speed);
        Vector3 velocity = Camera.main.transform.forward * speed;
        velocity += new Vector3(0, 10, 0);
        Debug.Log("VELOCITY******************************** " + velocity);
        hand.ThrowItem(velocity);
    }
}
