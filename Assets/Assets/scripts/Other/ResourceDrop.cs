using UnityEngine;
using System.Collections.Generic;

public class ResourceDrop : MonoBehaviour
{
    public ItemGenerator itemGenerator;
    public List<ResourceData> itemDrops;
    public List<GameObject> acquiredItems;
    public float minDropForce;
    public float maxDropForce;

    public void DropAllResources()
    {
        foreach (ResourceData data in itemDrops)
        {
            for (int i = 0; i < data.amount; i++)
            {
                itemGenerator.GenerateItem(acquiredItems, data.itemType, data.itemName);
            }
        }

        foreach (GameObject obj in acquiredItems)
        {
            DropResource(obj);
        }
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
