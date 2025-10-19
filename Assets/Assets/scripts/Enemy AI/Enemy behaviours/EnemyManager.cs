using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public string enemyID;
    private Rigidbody2D rb;
    private UniqueID IDManager;
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

        IDManager = GetComponent<UniqueID>();

        if (IDManager != null)
        {
            enemyID = IDManager.ID;

            var state = GetOrCreateEnemyState();

            if (state.isDead)
                Disable();
        }
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("hazard"))
        {
            Death();
        }
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
        enabled = false;

        if (IDManager != null)
        {
            var state = GetOrCreateEnemyState();
            state.isDead = true;
        }
    }

    private EnemyState GetOrCreateEnemyState()
    {
        var enemies = WorldPersistenceManager.Instance?.enemies;
        if (enemies == null)
        {
            Debug.LogWarning("No enemy list found in WorldPersistenceManager.");
            return null;
        }

        var state = enemies.Find(e => e.enemyID == enemyID);
        if (state == null)
        {
            state = new EnemyState { enemyID = enemyID, isDead = false };
            enemies.Add(state);
        }

        return state;
    }

    void Disable()
    {
        var state = GetOrCreateEnemyState();
        state.isDead = true;
        gameObject.layer = Mathf.RoundToInt(Mathf.Log(deathLayer.value, 2));
        rb.linearVelocityX = 0;
        hurtzone.SetActive(false);
        graphic.color = deathColor;
        movementLogic.enabled = false;
        enabled = false;
    }
}
