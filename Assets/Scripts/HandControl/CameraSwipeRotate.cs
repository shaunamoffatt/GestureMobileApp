using System;
using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Uses TouchScript ScreenTransformGesture to Rotate the camera around the X OR Y axis
/// Allows for Camera to move forward using two fingers
/// </summary>
public class CameraSwipeRotate : MonoBehaviour
{
    //Camera transform
    public Transform cam;
    public Transform hand;
    public ScreenTransformGesture transformGesture;

    //Screen swipe gesture to obtain components
    public float RotationSpeed = 5;
    private float MinClampAngle = -60;
    private float MaxClampAngle = 60;

    //Limits for where the user can swipe to look up and down
    private float limitScreenX;
    //  
    [SerializeField] RectTransform rightPanelImageRect;

    // Camera Bounds
    // X AND Z the same
    float maxX = 150;
    float minX = -150;
    // Y
    float minY = HandGestureUtils.minYCameraPos();
    float maxY = HandGestureUtils.maxYCameraPos();

    private void Awake()
    {
        cam = Camera.main.transform;
        limitScreenX = Screen.width - (Screen.width * 1 / 6f);
        //Using the semi transparent panel image
        //limitScreenX = Display.main.systemWidth - rightPanelImageRect.anchoredPosition.x;
    }

    private void OnEnable()
    {
        transformGesture = Camera.main.GetComponent<ScreenTransformGesture>();
        transformGesture.Transformed += TransformGesture_Transformed;
    }
    private void OnDisable()
    {
        transformGesture.Transformed -= TransformGesture_Transformed;
    }

    /// <summary>
    /// Camera Swipe Rotation
    /// </summary>
    private void TransformGesture_Transformed(object sender, EventArgs e)
    {
        Vector3 v = cam.rotation.eulerAngles;
        var deltax = (transformGesture.DeltaPosition.x / Screen.width) * 1000;
        var deltay = (transformGesture.DeltaPosition.y / Screen.height) * 1000;
        // 1 finger Camera Movements
        if (transformGesture.ActivePointers.Count == 1)
        {
            if (Math.Abs(deltax) >= Math.Abs(deltay))
            {
                // Rotate around the Y axis- CAmera Spin
                cam.rotation = Quaternion.Euler(v.x, v.y - (deltax * RotationSpeed * Time.deltaTime), v.z);
            }
            else
            {
                // Look up and Down- Limited to Screen Touches on The right side of the screen
                //if (transformGesture.ScreenPosition.x >= limitScreenX)
               
                GraphicRaycaster gr = rightPanelImageRect.GetComponent<GraphicRaycaster>();
                //Create the PointerEventData with null for the EventSystem
                PointerEventData ped = new PointerEventData(null);
                //Set required parameters
                ped.position = transformGesture.ScreenPosition;
                //Create list to receive all results
                List<RaycastResult> results = new List<RaycastResult>();
                //Raycast it
                gr.Raycast(ped, results);

                if (results.Count > 0)
                {
                    //limit the angles so the user cant do a full 360 in the vertical direction
                    var rot = v.x + (deltay * RotationSpeed * Time.deltaTime);
                    if (rot > 180) rot -= 360;
                    rot = Mathf.Clamp(rot, MinClampAngle, MaxClampAngle);

                    var rothand = Mathf.Clamp(rot, -5, 5);
                    // Rotate around the X axis - 
                    cam.rotation = Quaternion.Euler(rot, v.y, v.z);
                }
            }
        }
        else if (transformGesture.ActivePointers.Count == 2)
        {
            //Zoom the camera if 2 fingers are used
            cam.position += Camera.main.transform.forward * (transformGesture.DeltaScale - 1f) * 50;
            CameraInBounds();
        }
        }

        void CameraInBounds()
        {
            float camX = Mathf.Clamp(cam.position.x, minX, maxX);
            float camY = Mathf.Clamp(cam.position.y, minY, maxY);
            float camZ = Mathf.Clamp(cam.position.z, minX, maxX);
            cam.position = new Vector3(camX, camY, camZ);
        }
    }

