using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyState
{
    public string enemyID;
    public bool isDead;
}

[System.Serializable]
public class CheckpointState
{
    public string checkpointID;
    public bool isActivated;
}

[DefaultExecutionOrder(-100)]
public class WorldPersistenceManager : MonoBehaviour
{
    public static WorldPersistenceManager Instance;

    public List<EnemyState> enemies = new List<EnemyState>();
    public List<CheckpointState> checkpoints = new List<CheckpointState>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
