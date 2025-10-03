using UnityEngine;
using System;

[DefaultExecutionOrder(-50)]
public class UniqueID : MonoBehaviour
{
    [SerializeField] private string uniqueId;

    public string ID => uniqueId;

    private void Awake()
    {
        string key = gameObject.name + "_UniqueID"; // must be consistent
        if (PlayerPrefs.HasKey(key))
            uniqueId = PlayerPrefs.GetString(key);
        else
        {
            uniqueId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(key, uniqueId);
            PlayerPrefs.Save();
        }
    }
}