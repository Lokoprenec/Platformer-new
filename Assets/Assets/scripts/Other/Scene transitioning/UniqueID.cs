using UnityEngine;
using System;

[DefaultExecutionOrder(-50)]
public class UniqueID : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string uniqueId;

    public string ID => uniqueId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // If the ID is empty OR if multiple objects in the scene have the same ID, generate a new one
        if (string.IsNullOrEmpty(uniqueId) || HasDuplicate())
        {
            uniqueId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    private bool HasDuplicate()
    {
        var duplicates = FindObjectsOfType<UniqueID>();
        foreach (var obj in duplicates)
        {
            if (obj == this) continue;
            if (obj.ID == uniqueId) return true;
        }
        return false;
    }
#endif

    private void Awake()
    {
        // Runtime fallback: ensure ID exists
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Reset UniqueIDs in Scene")]
    private static void ResetAllIDs()
    {
        var all = FindObjectsOfType<UniqueID>();
        foreach (var u in all)
        {
            u.GetType().GetField("uniqueId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(u, Guid.NewGuid().ToString());
            UnityEditor.EditorUtility.SetDirty(u);
        }
        Debug.Log("Reset all UniqueIDs in scene.");
    }
#endif
}