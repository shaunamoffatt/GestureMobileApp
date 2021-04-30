using System.Collections;
using UnityEngine;

public class ScaleClick : MonoBehaviour
{
    [SerializeField]protected float scale = 1.15f;
    protected float scaleDuration = 0.2f;
    protected Vector3 initialScale;// = transform.localScale;

    private void Start()
    {
        initialScale = transform.localScale;
    }
    private void OnMouseDown()
    {
        StartCoroutine(ScaleUpAndDown());
    }


    protected virtual void PerfomOnClick() {

        //SoundManager.PlaySound(SoundManager.Sound.click);
    }

    public void Click()
    {
        StartCoroutine(ScaleUpAndDown());
    }

    // Adapted from answer here : https://answers.unity.com/questions/1752632/scale-with-coroutine-and-lerp.html
    protected IEnumerator ScaleUpAndDown()
    {
        Vector3 upScale = new Vector3(initialScale.x * scale, initialScale.y* scale, 0);
        Debug.Log("Scaling");
        for (float time = 0; time < scaleDuration; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, scaleDuration) / scaleDuration;
            transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
            yield return null;
        }
        transform.localScale = initialScale;
        PerfomOnClick();
        transform.localScale = initialScale;
    }
}
