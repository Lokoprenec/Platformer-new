using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Rigidbody2D rb;
    public ResourceDrop resourceDrop;
    public SpriteRenderer graphic;
    public MonoBehaviour movementLogic;
    public GameObject hurtzone;
    public float knockbackSensitivityMultiplier;
    public float knockbackTime;
    private float knockbackTimer;
    private bool knockbacked;
    public float knockbackStunTime;
    private float knockbackStunTimer;
    public float maxHealth;
    public float health;
    public Color deathColor;
    public LayerMask deathLayer;
    public Color hitColor;
    public Color normalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        graphic.color = normalColor;
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

        if (health <= 0 && knockbackTimer < 0)
        {
            Death();
        }
    }

    public void Knockback(float force, Vector2 direction)
    {
        knockbacked = true;
        movementLogic.enabled = false;
        rb.linearVelocity = force * direction;
        knockbackTimer = knockbackTime;
        knockbackStunTimer = knockbackStunTime;
        graphic.color = hitColor;
    }

    void EndKnockback()
    {
        knockbacked = false;
        movementLogic.enabled = true;
        graphic.color = normalColor;
    }

    void Death()
    {
        gameObject.layer = Mathf.RoundToInt(Mathf.Log(deathLayer.value, 2));

        if (resourceDrop != null)
        {
            resourceDrop.DropAllResources();
        }

        rb.linearVelocityX = 0;
        hurtzone.SetActive(false);
        graphic.color = deathColor;
        movementLogic.enabled = false;
        this.enabled = false;
    }
}
