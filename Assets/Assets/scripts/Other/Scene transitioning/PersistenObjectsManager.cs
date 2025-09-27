using UnityEngine;

public class PersistenObjectsManager : MonoBehaviour
{
    private static PersistenObjectsManager instance;

    private void Awake()
    {
        // Ensure only one player persists across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // keep this object when loading new scenes
        }
        else
        {
            Destroy(gameObject); // if another player spawned, destroy it
        }
    }
}
