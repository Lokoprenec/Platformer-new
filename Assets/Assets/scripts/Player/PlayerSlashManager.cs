using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerSlashManager : MonoBehaviour
{
    public PlayerAttackDirections currentDirection;
    public GameObject selectedSlashGraphic;
    [Header("slashGraphics with their corresponding collider: [side, up, down]")]
    public List<GameObject> slashGraphics;
    public List<Collider2D> colliders;
    public float slashCooldown;
    public float slashCooldownTimer;
    public float slashKnockback;
    public float slashDuration;
    public float slashTimer;
    public float damage;
    public List<GameObject> hitObjects;
    public List<GameObject> ignoredObjects;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hitObjects.Contains(collision.gameObject) || ignoredObjects.Contains(collision.gameObject)) return;
        hitObjects.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!hitObjects.Contains(collision.gameObject)) return;
        hitObjects.Remove(collision.gameObject);
    }

    public void ChangeAttackDirection(PlayerAttackDirections direction)
    {
        switch (direction)
        {
            case PlayerAttackDirections.Side:

                colliders[0].enabled = true;
                colliders[1].enabled = false;
                colliders[2].enabled = false;
                selectedSlashGraphic = slashGraphics[0];
                currentDirection = PlayerAttackDirections.Side;

                break;

            case PlayerAttackDirections.Up:

                colliders[0].enabled = false;
                colliders[1].enabled = true;
                colliders[2].enabled = false;
                selectedSlashGraphic = slashGraphics[1];
                currentDirection = PlayerAttackDirections.Up;

                break;

            case PlayerAttackDirections.Down:

                colliders[0].enabled = false;
                colliders[1].enabled = false;
                colliders[2].enabled = true;
                selectedSlashGraphic = slashGraphics[2];
                currentDirection = PlayerAttackDirections.Down;

                break;
        }

        foreach (GameObject graphic in slashGraphics)
        {
            graphic.SetActive(false);
        }
    }
}

public enum PlayerAttackDirections
{
    Side, Up, Down
}
