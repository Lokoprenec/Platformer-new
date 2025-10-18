using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public PlayerAbilities abilityType;
    public LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2))) return;
        PlayerManager pM = FindAnyObjectByType<PlayerManager>();
        pM.GetAbility(abilityType);
        Destroy(gameObject);
    }
}
