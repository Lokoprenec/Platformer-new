using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Rigidbody2D rb;
    public MonoBehaviour movementLogic;
    public float knockbackSensitivityMultiplier;
    public float knockbackTime;
    private float knockbackTimer;
    private bool knockbacked;
    public float knockbackStunTime;
    private float knockbackStunTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (knockbacked)
        {
            knockbackTimer -= Time.deltaTime;
            knockbackStunTimer -= Time.deltaTime;

            if (knockbackTimer < 0)
            {
                rb.linearVelocityX = 0;
            }

            if (knockbackStunTimer < 0)
            {
                EndKnockback();
            }
        }
    }

    public void Knockback(float force, float direction)
    {
        knockbacked = true;
        movementLogic.enabled = false;
        rb.linearVelocityX = force * direction;
        knockbackTimer = knockbackTime;
        knockbackStunTimer = knockbackStunTime;
    }

    void EndKnockback()
    {
        knockbacked = false;
        movementLogic.enabled = true;
    }
}
