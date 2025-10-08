using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [Header("Health")]
    private SpriteRenderer sR;
    public PlayerController pC;
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
    public string checkpointID = null;
    public Vector2 respawnPos;
    public Color activeCheckpointColor;
    public Color inactiveCheckpointColor;
    [SceneName]
    private string respawnScene;
    public float hazardCheckpointCooldown;
    private float hazardCheckpointTimer;
    private Vector2 hazardCheckpoint;

    [Header("Item collection")]
    public int dabloonCount;
    public TextMeshProUGUI text;

    [Header("Room traversal")]
    public float entranceOffset;
    public LayerMask groundLayer;
    private BoxCollider2D col;
    private CinemachinePositionComposer cam;

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
        pC = GetComponent<PlayerController>();
        sR = pC.graphic.GetComponent<SpriteRenderer>();
        cam = GameObject.Find("CinemachineCamera").GetComponent<CinemachinePositionComposer>();

        foreach (Transform component in healthBar.transform)
        {
            healthBarComponents.Add(component);
        }

        health = maxHealth;

        // Load last checkpoint if it exists
        var state = WorldPersistenceManager.Instance?.checkpoints?
            .Find(e => e.checkpointID == checkpointID);

        if (state == null)
        {
            // fallback to default spawn only if no checkpoint
            respawnPos = transform.position;
            respawnScene = SceneManager.GetActiveScene().name;
        }

        SetHazardCheckpointWhenGrounded();
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

        if (pC.knockbackedStunTimer <= 0)
        {
            CheckForDeath();
        }

        text.text = dabloonCount.ToString();

        if (hazardCheckpointTimer >= 0)
        {
            hazardCheckpointTimer -= Time.deltaTime;
        }
        else
        {
            SetHazardCheckpointWhenGrounded();
        }
    }

    void SetHazardCheckpointWhenGrounded()
    {
        if (pC.isAbsolutelySafelyGrounded)
        {
            hazardCheckpoint = transform.position;
            hazardCheckpointTimer = hazardCheckpointCooldown;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("hazard"))
        {
            invincibilityTimer = invincibilityCooldown;
            sR.color = invincibilityColor;
            takeDamageFromHazard();
        }
        else if (invincibilityTimer < 0)
        {

            if (collision.gameObject.CompareTag("contact damage"))
            {
                health -= collision.gameObject.GetComponent<DamageInfo>().contactDamage;

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
            }
            else
            {
                return;
            }

            invincibilityTimer = invincibilityCooldown;
            sR.color = invincibilityColor;
        }
    }

    void takeDamageFromHazard()
    {
        health -= 1;
        transform.position = hazardCheckpoint;
        pC.knockbackedStunTimer = 0;
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
        LoadRespawnScene();
    }

    public void LoadRespawnScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(respawnScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Respawn();
        SceneManager.sceneLoaded -= OnSceneLoaded; // unsubscribe so it doesn’t stack
    }

    public void Respawn()
    {
        transform.position = respawnPos;
        health = maxHealth;
        pC.currentMovementState = MovementStates.Idle;
        sR.color = regularColor;
        gameObject.SetActive(true);
        PositionOnTheGround();
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
        if (collision.CompareTag("checkpoint") && enabled)
        {
            SetCheckpoint(collision.gameObject);
        }
    }

    void SetCheckpoint(GameObject point)
    {
        if (checkpointID != null)
        {
            var state = WorldPersistenceManager.Instance.checkpoints
            .Find(e => e.checkpointID == checkpointID);

            if (state == null)
            {
                WorldPersistenceManager.Instance.checkpoints.Add(
                    new CheckpointState { checkpointID = checkpointID, isActivated = false });
            }
            else
            {
                state.isActivated = false;
            }

            CheckpointController[] cCs = FindObjectsOfType<CheckpointController>();

            foreach (CheckpointController cC in cCs)
            {
                if (cC.checkpointID == checkpointID)
                {
                    cC.Deactivate();
                }
            }
        }

        CheckpointController checkpoint = point.GetComponent<CheckpointController>();
        checkpointID = checkpoint.checkpointID;
        respawnPos = checkpoint.transform.position;
        checkpoint.Activate();
        respawnScene = SceneManager.GetActiveScene().name;
        health = maxHealth;
    }

    public void EnterNewScene(Transform enter, EntranceDirections direction)
    {
        Vector2 offset = Vector2.zero;

        StartCoroutine(CameraSetup());

        switch (direction)
        {
            case EntranceDirections.Left:

                offset = new Vector2(-entranceOffset, 0);

                break;
            case EntranceDirections.Right:

                offset = new Vector2(entranceOffset, 0);

                break;
            case EntranceDirections.Top:

                offset = new Vector2(0, entranceOffset);

                break;
            case EntranceDirections.Bottom:

                offset = new Vector2(0, -entranceOffset);

                break;
        }

        transform.position = new Vector2(enter.position.x + offset.x, enter.position.y + offset.y);
        enabled = true;
        pC.enabled = true;
        PositionOnTheGround();
    }

    private IEnumerator CameraSetup()
    {
        // Copy the structs
        var dz = cam.Composition.DeadZone;
        var hl = cam.Composition.HardLimits;

        // Assign the modified structs back
        cam.Composition.DeadZone.Size.y = 0;
        cam.Composition.HardLimits.Size.y = 0;

        yield return new WaitForSecondsRealtime(0.5f);

        // Now reassign
        cam.Composition.DeadZone = dz;
        cam.Composition.HardLimits = hl;
    }

    private void PositionOnTheGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            float groundTop = hit.collider.bounds.max.y;   // top of ground in world space
            float halfHeight = col.bounds.extents.y;       // half height of player

            float newY = groundTop + halfHeight;

            transform.position = new Vector2(transform.position.x, newY);
        }

        pC.currentMovementState = MovementStates.Idle;
    }
}
