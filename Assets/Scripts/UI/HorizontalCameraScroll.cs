using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalCameraScroll : MonoBehaviour
{
    [SerializeField] Image[] circles;
    int currentPos = 0;
    [SerializeField] int numberOfInstructions = 8;
    [SerializeField] float distanceX = 20;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveCameraLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveCameraRight();
        }
    }
    private void Start()
    {
        currentPos = 0;
        // Set first dot to green
        circles[0].color = Color.green;
    }

    public void MoveCameraRight()
    {
        if (currentPos < numberOfInstructions - 1)
        {
            StartCoroutine(LerpCamera(1f, 1f));
            currentPos++;
            circles[currentPos].color = Color.green;
        }
    }

    public void MoveCameraLeft()
    {
        if (currentPos > 0)
        {
            circles[currentPos].color = Color.white;
            StartCoroutine(LerpCamera(1f, -1f));
            currentPos--;
        }
    }

    IEnumerator LerpCamera(float duration, float sign)
    {
        float x = (currentPos * 20f) + (20 * sign);
        Vector3 pos = new Vector3(x, 0, 0);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, pos, t / duration);
            yield return 0;
        }
        Camera.main.transform.position = pos;
    }

}
