using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public string checkpointID;
    private UniqueID IDManager;
    public Color deactivationColor;
    public Color activationColor;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        IDManager = GetComponent<UniqueID>();

        checkpointID = IDManager.ID;

        var state = GetOrCreateCheckpointState();

        if (state.isActivated)
            Activate();
        else
            Deactivate();
    }

    public void Activate()
    {
        var state = GetOrCreateCheckpointState();
        state.isActivated = true;
        sr.color = activationColor;
    }

    public void Deactivate()
    {
        var state = GetOrCreateCheckpointState();
        state.isActivated = false;
        sr.color = deactivationColor;
    }

    private CheckpointState GetOrCreateCheckpointState()
    {
        var checkpoints = WorldPersistenceManager.Instance?.checkpoints;
        if (checkpoints == null)
        {
            Debug.LogWarning("No checkpoints list found in WorldPersistenceManager.");
            return null;
        }

        var state = checkpoints.Find(e => e.checkpointID == checkpointID);
        if (state == null)
        {
            state = new CheckpointState { checkpointID = checkpointID, isActivated = false };
            checkpoints.Add(state);
        }

        return state;
    }
}