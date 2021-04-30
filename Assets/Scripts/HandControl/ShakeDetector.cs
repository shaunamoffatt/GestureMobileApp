using UnityEngine;

/// <summary>
/// 
/// 
/// </summary>

public class ShakeDetector : MonoBehaviour
{
    [SerializeField] public float ShakeDetectionThreshold = 3.6f;
    [SerializeField] public float MinShakeInterval = 0.5f;

    private float sqrShakeDetectionThreshold;
    private float timeSinceLastShake;

    private GestureActivationController gestureActivation;
    // Start is called before the first frame update
    void Start()
    {
        sqrShakeDetectionThreshold = Mathf.Pow(ShakeDetectionThreshold, 2);
        gestureActivation = gameObject.GetComponent<GestureActivationController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.acceleration.sqrMagnitude >= sqrShakeDetectionThreshold
            && Time.unscaledTime >= timeSinceLastShake + MinShakeInterval)
        {
            //Set Time since last shake;
            timeSinceLastShake = Time.unscaledTime;

            //Send Message to MagicController to do its thing( enable and disable certain Gesture Scripts)
            gestureActivation.ToggleActiveGestures();
        }
    }

}