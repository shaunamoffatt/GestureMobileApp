using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activate and Deactivate certain components on the Gameobject HandGestureControl. 
/// The class ShakeDetector calls the ToggleActiveGesture to switch between modes when the device has been shook.  
/// (Moving and Magic)
/// </summary>
public class GestureActivationController : MonoBehaviour
{
    private GameObject HandGestureControl;
    private List<MonoBehaviour> components;
    // The UI Canvas contains the arrows and semi-transparent right pane;
    [SerializeField] private GameObject UICanvas;
    // The Magic Canvas contains the Spell shape Images
    [SerializeField] private GameObject MagicCanvas;
    // The BorderEffects contains the particle systems for the magic mode display
    [SerializeField] private GameObject BorderEffects;
    ParticleSystem[] particles;
    private bool magic = false;

    HandController hand;
    // Start is called before the first frame update
    private void Start()
    {
        hand = GameObject.Find("hand").GetComponent<HandController>();
        components = new List<MonoBehaviour>();
        GetComponents();
        if (BorderEffects == null)
        {
            Debug.LogError("Forgot to add BorderEffects object to the GestureActivationController");
        }
        else
        {
            particles = BorderEffects.GetComponentsInChildren<ParticleSystem>();
            DeactivateBorder();
        }
    }
    private void GetComponents()
    {
        HandGestureControl = GameObject.FindGameObjectWithTag("gesture control");
        try
        {
            //Add all the components that the handGesture Control has
            components.Add(HandGestureControl.GetComponent<DoubleTapAction>());
            components.Add(HandGestureControl.GetComponent<CameraSwipeRotate>());
            components.Add(HandGestureControl.GetComponent<CameraLongPressMove>());
            components.Add(HandGestureControl.GetComponent<SwipeFlick>());
            //Theses last 2 will be disabled when the other four are enabled (toggled)
            components.Add(HandGestureControl.GetComponent<SwipeTrail>());
            components.Add(HandGestureControl.GetComponent<GestureRecognitionSwipe>());
        }
        catch
        {
            Debug.LogWarning("Seems to be no Gesture Control components set yet");
        }
    }

    public void ToggleActiveGestures()
    {
        switch (hand.getState())
        {
            case HandState.Holding:
                {
                    //Drop what you are holding at the hands location
                    hand.DropItem(Camera.main.transform.position + HandGestureUtils.handOffset());
                    break;
                }
            default://Only allow to use magic when not holding something or Idle
                {
                    foreach (MonoBehaviour c in components)
                    {
                        c.enabled = !c.enabled;
                    }

                    //Toggle magic
                    magic = !magic;
                  
                    //Toggle the Magic Particles on the MagicCamera
                    if (magic && particles != null)
                        ActivateBorder();
                    else
                        DeactivateBorder();
                    break;
                }
        }

    }

    void ActivateBorder()
    {
        foreach (ParticleSystem p in particles)
        {
            p.Play();
        }
        UICanvas.SetActive(false);
        MagicCanvas.SetActive(true);
    }

    void DeactivateBorder()
    {
        foreach (ParticleSystem p in particles)
        {
            p.Stop();
        }
        UICanvas.SetActive(true);
        MagicCanvas.SetActive(false);
    }
}
