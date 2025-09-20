using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerSlashManager : MonoBehaviour
{
    public GameObject slashGraphic;
    public float slashCooldown;
    public float slashCooldownTimer;
    public float slashKnockback;
    public float slashDuration;
    public float slashTimer;
    public float damage;
    public List<GameObject> hitObjects;
    public List<GameObject> ignoredObjects;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitObjects.Contains(collision.gameObject) || ignoredObjects.Contains(collision.gameObject)) return;
        hitObjects.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!hitObjects.Contains(collision.gameObject)) return;
        hitObjects.Remove(collision.gameObject);
    }
}
