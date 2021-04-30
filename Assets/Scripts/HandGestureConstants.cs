using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Class to Keep Control of some of the constants
public static class HandGestureUtils 
{
    public static float cameraMoveSpeed()
    {
        return 10;
    }

    public static float minYCameraPos()
    {
        return 25;
    }

    public static float maxYCameraPos()
    {
        return 250;
    }
    public static Vector3 handOffset()
    {
        return new Vector3(0, -5f, 10);
    }

    public static Vector3 holdingLocalPostition()
    {
        return new Vector3(-0.5f, 0.2f, -1);
    }
}
