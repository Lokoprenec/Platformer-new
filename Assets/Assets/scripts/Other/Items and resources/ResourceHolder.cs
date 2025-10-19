using UnityEngine;

public class ResourceHolder : MonoBehaviour
{
    private UniqueID IDManager;
    public string resourceDropID;
    public ResourceDrop resourceDrop;
    private SpriteRenderer graphic;
    public float maxHealth;
    public float health;
    public Color deathColor;
    public LayerMask deathLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        graphic = GetComponent<SpriteRenderer>();

        IDManager = GetComponent<UniqueID>();

        if (IDManager != null)
        {
            resourceDropID = IDManager.ID;

            var state = GetOrCreateResourceDropState();

            maxHealth = state.health;
            health = maxHealth;

            if (state.health <= 0)
            {
                Deactivate();
            }
            else
            {
                for (int i = 0; i < state.data.Count; i++)
                {
                    ResourceData data = resourceDrop.itemDrops[i];
                    data.amountLeft = state.data[i].amountLeft;
                }
            }
        }
    }
    
    public void Hit(float damageTaken)
    {
        if (health <= 0)
        {
            resourceDrop.DropAllResources();
            Deactivate();

            if (IDManager != null)
            {
                var state = GetOrCreateResourceDropState();
                state.health = health;
            }
        }
        else
        {
            for (int i = 0; i < resourceDrop.itemDrops.Count; i++)
            {
                ResourceData data = resourceDrop.itemDrops[i];
                int amountToDrop = Mathf.FloorToInt(data.amount / (maxHealth / damageTaken));
                resourceDrop.DropSomeOfTheResource(amountToDrop, i);

                if (IDManager != null)
                {
                    var state = GetOrCreateResourceDropState();
                    state.data[i].amountLeft = data.amountLeft;
                    state.health = health;
                }
            }
        }
    }

    void Deactivate()
    {
        gameObject.layer = Mathf.RoundToInt(Mathf.Log(deathLayer.value, 2));
        graphic.color = deathColor;
        enabled = false;
    }

    private ResourceDropState GetOrCreateResourceDropState()
    {
        var resourceDrops = WorldPersistenceManager.Instance?.resourceDrops;
        if (resourceDrops == null)
        {
            Debug.LogWarning("No resourceDrop list found in WorldPersistenceManager.");
            return null;
        }

        var state = resourceDrops.Find(e => e.resourceDropID == resourceDropID);
        if (state == null)
        {
            state = new ResourceDropState
            {
                resourceDropID = resourceDropID,
                health = maxHealth,
                data = resourceDrop.itemDrops
            };
            resourceDrops.Add(state);
        }

        return state;
    }
}
