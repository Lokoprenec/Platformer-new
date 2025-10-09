using UnityEngine;

public class ResourceHolder : MonoBehaviour
{
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
        health = maxHealth;
    }
    
    public void Hit(float damageTaken)
    {
        if (health <= 0)
        {
            resourceDrop.DropAllResources();
            Debug.Log("resourcesDropped");
            Deactivate();
        }
        else
        {
            for (int i = 0; i < resourceDrop.itemDrops.Count; i++)
            {
                ResourceData data = resourceDrop.itemDrops[i];
                resourceDrop.DropSomeOfTheResource(Mathf.FloorToInt(data.amount / (maxHealth / damageTaken)), i);
            }
        }
    }

    void Deactivate()
    {
        gameObject.layer = Mathf.RoundToInt(Mathf.Log(deathLayer.value, 2));
        graphic.color = deathColor;
        this.enabled = false;
    }
}
