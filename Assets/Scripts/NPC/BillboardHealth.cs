using UnityEngine;
using UnityEngine.UI;

public class BillboardHealth : MonoBehaviour
{
    public Canvas HealthCanvas;
    public Image healthBar;
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * -Vector3.back,
            Camera.main.transform.rotation * -Vector3.down);
    }

    public void setHealth()
    {

    }

}
