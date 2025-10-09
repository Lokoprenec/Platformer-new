using UnityEngine;
using System.Collections.Generic;

public class ResourceDrop : MonoBehaviour
{
    private ItemGenerator itemGenerator;
    public List<ResourceData> itemDrops;
    public List<GameObject> acquiredItems;
    public float minDropForce;
    public float maxDropForce;

    private void Start()
    {
        foreach (ResourceData data in itemDrops)
        {
            data.amountLeft = data.amount;
        }
    }

    public void DropAllResources()
    {
        itemGenerator = FindObjectOfType<ItemGenerator>();

        foreach (ResourceData data in itemDrops)
        {
            for (int i = data.amountLeft; i > 0; i--)
            {
                itemGenerator.GenerateItem(acquiredItems, data.itemType, data.itemName);
                data.amountLeft -= 1;
            }
        }

        foreach (GameObject obj in acquiredItems)
        {
            DropResource(obj);
        }

        acquiredItems.Clear();
    }

    public void DropSomeOfTheResource(int amount, int typeIndex)
    {
        itemGenerator = FindObjectOfType<ItemGenerator>();
        ResourceData data = itemDrops[typeIndex];

        for (int i = amount; i > 0; i--)
        {
            itemGenerator.GenerateItem(acquiredItems, data.itemType, data.itemName);
            data.amountLeft -= 1;
        }

        foreach (GameObject obj in acquiredItems)
        {
            DropResource(obj);
        }

        acquiredItems.Clear();
    }

    void DropResource(GameObject item)
    {
        item.transform.position = transform.position;
        item.SetActive(true);
        Rigidbody2D itemRB = item.GetComponent<Rigidbody2D>();
        float randomForce = Random.Range(minDropForce, maxDropForce);
        float coneAngle = 60f;
        float randomAngle = Random.Range(-coneAngle / 2f, coneAngle / 2f);
        Vector2 dir = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;
        itemRB.AddForce(randomForce * dir, ForceMode2D.Impulse);
    }
}
