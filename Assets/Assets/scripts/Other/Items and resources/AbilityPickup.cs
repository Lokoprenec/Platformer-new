using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public PlayerAbilities abilityType;
    public LayerMask playerLayer;

    private void Start()
    {
        var state = GetOrCreateAbilityState();

        if (state.isAcquired)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2))) return;
        PlayerManager pM = FindAnyObjectByType<PlayerManager>();
        pM.GetAbility(abilityType);
        var state = GetOrCreateAbilityState();
        state.isAcquired = true;
        Destroy(gameObject);
    }

    private AbilityState GetOrCreateAbilityState()
    {
        var abilities = WorldPersistenceManager.Instance?.abilities;
        if (abilities == null)
        {
            Debug.LogWarning("No ability list found in WorldPersistenceManager.");
            return null;
        }

        var state = abilities.Find(e => e.abilityType == abilityType);
        if (state == null)
        {
            state = new AbilityState { abilityType = abilityType, isAcquired = false };
            abilities.Add(state);
        }

        return state;
    }
}
