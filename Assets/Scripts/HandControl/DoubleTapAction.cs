using System;
using TouchScript.Gestures;
using UnityEngine;

public class DoubleTapAction : MonoBehaviour
{
    public TapGesture doubleTapGesture;
    private Ray ray;
    private RaycastHit hit;
    [SerializeField] float minGrabDistance = 40;
    public HandController hand;
    public float flickForce = 50;

    private void OnEnable()
    {
        doubleTapGesture = Camera.main.GetComponent<TapGesture>();
        doubleTapGesture.Tapped += DoubleTapGesture_Activate;
    }

    private void OnDisable()
    {
        doubleTapGesture.Tapped -= DoubleTapGesture_Activate;
    }

    private void DoubleTapGesture_Activate(object sender, EventArgs e)
    {
        //Check for collision with item that can be picked up
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();
        //Check if holding
        if (hand.HoldingObject())
        {
            DropItem();
        }
        else
        {
            TrytoPickUpItem();
        }
    }

    private void TrytoPickUpItem()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.gameObject.tag == "pickable")
        {
            hand.GrabItem(hit.collider.gameObject);
        }
    }

    private void DropItem()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.gameObject.tag == "ground")
        {
            hand.DropItem(hit.point);
        }
    }
}
