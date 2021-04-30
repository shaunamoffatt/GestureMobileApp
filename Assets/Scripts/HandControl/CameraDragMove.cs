using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

public class DragMove : MonoBehaviour
{
    //Camera transform
    private Transform cam;

    public ScreenTransformGesture transformGesture;

    //Screen swipe gesture to obtain components
    public float RotationSpeed = 8;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    private void OnEnable()
    {
        transformGesture.Transformed += TransformGesture_Transformed;
    }
    private void OnDisable()
    {
        transformGesture.Transformed -= TransformGesture_Transformed;
    }

    /// <summary>
    /// Camera Swipe Rotation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
                // Rotate around the Y axis
                cam.rotation = Quaternion.Euler(v.x, v.y - (deltax * RotationSpeed * Time.deltaTime), v.z);
            }
            else
            {
                //limit the angles so the user cant do a full 360 in the vertical direction
                var rot = v.x + (deltay * RotationSpeed * Time.deltaTime);
                if (rot > 180) rot -= 360;
                rot = Mathf.Clamp(rot, -45, 35);
                // Rotate around the X axis - 
                if (cam.rotation.x < 45 || cam.rotation.x > -45)
                    cam.rotation = Quaternion.Euler(rot, v.y, v.z);
            }
        }
    }
}
