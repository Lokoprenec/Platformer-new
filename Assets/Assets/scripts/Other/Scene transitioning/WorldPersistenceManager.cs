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

[System.Serializable]
public class AbilityState
{
    public PlayerAbilities abilityType;
    public bool isAcquired;
}

[System.Serializable]
public class ResourceDropState
{
    public string resourceDropID;
    public float health;
    public List<ResourceData> data;
}

[DefaultExecutionOrder(-100)]
public class WorldPersistenceManager : MonoBehaviour
{
    public static WorldPersistenceManager Instance;

    public List<EnemyState> enemies = new List<EnemyState>();
    public List<CheckpointState> checkpoints = new List<CheckpointState>();
    public List<AbilityState> abilities = new List<AbilityState>();
    public List<ResourceDropState> resourceDrops = new List<ResourceDropState>();

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

    public void ResetAllEnemyStates()
    {
        foreach (EnemyState enemy in enemies)
        {
            if (!enemy.isDead) continue;
            enemy.isDead = false;
        }
    }
}
