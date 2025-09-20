using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public PlayerManager manager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        manager.dabloonCount += 1;
        collision.gameObject.SetActive(false);
        collision.transform.parent.GetComponent<ObjectPooling>().stored.Add(collision.gameObject);
    }
}
