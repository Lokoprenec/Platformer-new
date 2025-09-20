using UnityEngine;
using System.Collections.Generic;

public class ItemGenerator : MonoBehaviour
{
    public GameObject itemPool;
    public ItemTypes itemType;
    public ItemNames itemName;
    [SerializeField] private List<GameObject> stack;
    private GameObject chosenItem;
    private ObjectPooling objectPool;

    public void GenerateItem(List<GameObject> targetStack, ItemTypes targetItemType, ItemNames targetItemName)
    {
        itemType = targetItemType;
        itemName = targetItemName;
        stack = targetStack;
        GetItemFromPool();
    }

    void GetItemFromPool()
    {
        Transform typeTransform = FindChildByName(itemPool.transform, itemType.ToString());
        if (typeTransform == null) return;

        Transform subtypeTransform = FindChildByName(typeTransform, itemName.ToString());
        if (subtypeTransform == null) return;

        objectPool = subtypeTransform.GetComponent<ObjectPooling>();
        if (objectPool == null || objectPool.stored == null) return;

        objectPool.stored.RemoveAll(item => item == null);
        if (objectPool.stored.Count == 0) return;

        chosenItem = objectPool.stored[0];
        TransferItemToStack();
    }

    void TransferItemToStack()
    {
        stack.Add(chosenItem);
        DeleteItemFromPool();
    }

    void DeleteItemFromPool()
    {
        objectPool?.stored?.Remove(chosenItem);
        chosenItem = null;
        objectPool = null;
    }

    Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.name == name)
                return child;
        }

        return null;
    }
}

public enum ItemTypes
{
    Currency
}

public enum ItemNames
{
    Dabloons
}
