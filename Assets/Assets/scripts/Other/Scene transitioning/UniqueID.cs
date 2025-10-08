using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class UniqueID : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string uniqueId;

    public string ID => uniqueId;

    private const string PlayerPrefsKey = "UniqueID_Registry";

    // Local cache to avoid reloading PlayerPrefs every time
    private static HashSet<string> registeredIDs;

    private void Start()
    {
        var duplicates = FindObjectsOfType<UniqueID>();
        foreach (var obj in duplicates)
        {
            if (obj != this && obj.ID == uniqueId)
            {
                Debug.LogWarning($"Duplicate UniqueID found on {gameObject.name} and {obj.gameObject.name}");
            }
        }
    }

    private void Awake()
    {
        EnsureRegistryLoaded();
        EnsureID();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureRegistryLoaded();
        EnsureID();
    }
#endif

    private void EnsureRegistryLoaded()
    {
        if (registeredIDs != null) return;

        registeredIDs = new HashSet<string>();
        string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (!string.IsNullOrEmpty(saved))
        {
            string[] ids = saved.Split('|');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    registeredIDs.Add(id);
            }
        }
    }

    private void EnsureID()
    {
        if (string.IsNullOrEmpty(uniqueId) || !registeredIDs.Contains(uniqueId))
        {
            // Generate new unique GUID and register it
            uniqueId = Guid.NewGuid().ToString();
            registeredIDs.Add(uniqueId);
            SaveRegistry();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    private void SaveRegistry()
    {
        string joined = string.Join("|", registeredIDs);
        PlayerPrefs.SetString(PlayerPrefsKey, joined);
        PlayerPrefs.Save();
    }
}