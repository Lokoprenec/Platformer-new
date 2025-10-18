using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public PlayerManager manager;
    public LayerMask itemLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Mathf.RoundToInt(Mathf.Log(itemLayer.value, 2))) return;
        manager.dabloonCount += 1;
        collision.gameObject.SetActive(false);
        collision.transform.parent.GetComponent<ObjectPooling>().stored.Add(collision.gameObject);
    }
}
