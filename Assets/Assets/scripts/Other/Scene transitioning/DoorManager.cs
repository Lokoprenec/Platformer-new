using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorManager : MonoBehaviour
{
    [SceneName]
    public string sceneToLoad;
    public int transitionIndex;
    private Transform exitDoor;
    private PlayerManager pM;
    public EntranceDirections entranceDirection;
    private Animator sceneTransition;

    private void Start()
    {
        pM = FindAnyObjectByType<PlayerManager>();
        sceneTransition = GameObject.Find("SceneTransition").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayExitTransition();
    }

    public void PlayExitTransition()
    {
        pM = FindAnyObjectByType<PlayerManager>();
        pM.enabled = false;
        pM.pC.enabled = false;
        StartCoroutine(ExitTransitionCoroutine());
    }

    private IEnumerator ExitTransitionCoroutine()
    {
        // Play the exit animation
        sceneTransition.Play("exitScene");

        // Wait until the animation is done
        AnimatorStateInfo stateInfo = sceneTransition.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        yield return new WaitForSeconds(clipLength);

        // Load the new scene
        yield return StartCoroutine(LoadNewScene(sceneToLoad));
    }

    public void PlayEnterTransition()
    {
        sceneTransition.Play("enterScene");
    }

    private IEnumerator LoadNewScene(string newScene)
    {
        // Remember old scene
        Scene oldScene = SceneManager.GetActiveScene();

        GameObject[] sceneManagers = GameObject.FindGameObjectsWithTag("sceneManager");

        foreach (var manager in sceneManagers)
        {
            manager.SetActive(false);
        }

        // Load new scene additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        yield return loadOp;

        // Set the new scene as active
        Scene newLoadedScene = SceneManager.GetSceneByName(newScene);
        SceneManager.SetActiveScene(newLoadedScene);

        // Wait 1 frame to ensure everything is initialized
        yield return null;

        PlayEnterTransition();

        // Find only "door holder" objects that belong to the new scene
        GameObject[] allDoorHolders = GameObject.FindGameObjectsWithTag("door holder");
        GameObject doorHolder = null;

        foreach (var obj in allDoorHolders)
        {
            if (obj.scene == newLoadedScene)
            {
                doorHolder = obj;
                break;
            }
        }

        if (doorHolder == null)
        {
            Debug.LogError("No door holder found in scene: " + newLoadedScene.name);
            yield break;
        }

        // Get doors from the correct holder
        Transform[] doors = doorHolder.GetComponentsInChildren<Transform>();

        for (int i = 1; i < doors.Length; i++) // skip the root
        {
            DoorManager manager = doors[i].GetComponent<DoorManager>();

            if (manager != null && manager.sceneToLoad == oldScene.name && manager.transitionIndex == transitionIndex)
            {
                exitDoor = doors[i];

                // Refresh PlayerManager reference
                pM = FindAnyObjectByType<PlayerManager>();

                if (pM != null)
                {
                    pM.EnterNewScene(exitDoor, entranceDirection);
                }
                else
                {
                    Debug.LogError("Player Manager isn't assigned!");
                }

                break;
            }
        }

        // Now unload the old scene safely
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
        yield return unloadOp;

        foreach (var manager in sceneManagers)
        {
            if (manager == null) continue;
            manager.SetActive(true);
        }
    }
}

public enum EntranceDirections
{
    Left, Right, Top, Bottom
}
