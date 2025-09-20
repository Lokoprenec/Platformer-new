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

    [Header("Item collection")]
    public int dabloonCount;
    public TextMeshProUGUI text;

    private void Awake()
    {
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
            invincibilityTimer = invincibilityCooldown;

            if (collision.gameObject.CompareTag("hazard"))
            {
                Death();
            }
            else if (collision.gameObject.CompareTag("contact damage"))
            {
                health -= collision.gameObject.GetComponent<DamageInfo>().contactDamage;
            }

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
        transform.position = new Vector2(-12.69f, 6.56f);
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
}
