using UnityEngine;
using UnityEngine.UI;

public class BillboardHealth : MonoBehaviour
{
    public Image healthBar;
    public  Gradient gradient;
    float MaxHealth;
    float health;
    public Transform ragdollTransform;
    Rigidbody ragDollPelvis;
    private void Start()
    {
        ragDollPelvis = ragdollTransform.gameObject.GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * -Vector3.back,
            Camera.main.transform.rotation * -Vector3.down);

        healthBar.fillAmount = health/MaxHealth;
        healthBar.color = gradient.Evaluate(health/MaxHealth);
    }

    public void SetHealth(int health)
    {
        this.health = health;  
    }

    public void SetMaxHealth(int max)
    {
        MaxHealth = max;
        health = MaxHealth;
    }

    public void SetHealthBarPosition(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, transform.position.y, pos.z); ;
    }
}
