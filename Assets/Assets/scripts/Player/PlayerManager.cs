using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("Health")]
    private SpriteRenderer sR;
    private PlayerController pC;
    public GameObject healthBar;
    [SerializeField] private List<Transform> healthBarComponents;
    public Color activeHealth;
    public Color inactiveHealth;
    public float health;
    public float maxHealth;
    public float invincibilityCooldown;
    private float invincibilityTimer = 0;
    public Color regularColor;
    public Color invincibilityColor;
    public Transform checkpoint;
    public Vector2 respawnPos;
    public Color activeCheckpointColor;
    public Color inactiveCheckpointColor; 
    private static PlayerManager instance;

    [Header("Item collection")]
    public int dabloonCount;
    public TextMeshProUGUI text;

    private void Awake()
    {
        // Ensure only one player persists across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // keep this object when loading new scenes
        }
        else
        {
            Destroy(gameObject); // if another player spawned, destroy it
        }

        respawnPos = transform.position;
        pC = GetComponent<PlayerController>();
        sR = pC.graphic.GetComponent<SpriteRenderer>();

        foreach (Transform component in healthBar.transform)
        {
            healthBarComponents.Add(component);
        }

        health = maxHealth;
    }

    private void Update()
    {
        if (invincibilityTimer >= 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        else
        {
            sR.color = regularColor;
        }

        UpdateBar(healthBarComponents, activeHealth, inactiveHealth, health, maxHealth);

        if (pC.knockbackedStunTimer < 0)
        {
            CheckForDeath();
        }

        text.text = dabloonCount.ToString();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (invincibilityTimer < 0)
        {
            if (collision.gameObject.CompareTag("hazard"))
            {
                Death();
            }
            else if (collision.gameObject.CompareTag("contact damage"))
            {
                health -= collision.gameObject.GetComponent<DamageInfo>().contactDamage;
            }
            else
            {
                return;
            }

            invincibilityTimer = invincibilityCooldown;

            if (collision.transform.position.x < transform.position.x)
            {
                pC.knockbackedXDir = 1;
            }
            else
            {
                pC.knockbackedXDir = -1;
            }

            pC.knockbackedStunTimer = pC.knockbackedStun;
            pC.knockbackedTimer = pC.knockbackedCooldown;
            pC.currentMovementState = MovementStates.Knockbacked;
            sR.color = invincibilityColor;
        }
    }

    public void CheckForDeath()
    {
        if (health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Respawn();
    }

    public void Respawn()
    {
        transform.position = respawnPos;
        health = maxHealth;
        pC.currentMovementState = MovementStates.Idle;
        sR.color = regularColor;
    }

    public void UpdateBar(List<Transform> list, Color active, Color inactive, float value, float maxValue)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Image image = list[i].GetComponent<Image>();
            
            if (i < maxValue)
            {
                list[i].gameObject.SetActive(true);

                if (i >= value)
                {
                    image.color = inactive;
                }
                else
                {
                    image.color = active;
                }
            }
            else
            {
                list[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("checkpoint"))
        {
            SetCheckpoint(collision.gameObject);
        }
    }

    void SetCheckpoint(GameObject point)
    {
        if (checkpoint != null)
        {
            checkpoint.GetComponent<SpriteRenderer>().color = inactiveCheckpointColor;
        }

        checkpoint = point.transform;
        respawnPos = checkpoint.transform.position;
        checkpoint.GetComponent<SpriteRenderer>().color = activeCheckpointColor;
    }
}
