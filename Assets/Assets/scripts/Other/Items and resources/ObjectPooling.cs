using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    public List<GameObject> stored = new List<GameObject>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        stored.Clear();
        StartCoroutine(DisableChildrenNextFrame());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator DisableChildrenNextFrame()
    {
        yield return null;
        foreach (Transform t in transform)
        {
            stored.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Safety check
        if (this == null || gameObject == null) return;

        foreach (var go in stored)
        {
            if (go != null)
                go.SetActive(false);
        }

        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }
}