using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SceneName]
    public string startingScene;
    private KeybindManager keybindManager;
    public GameObject Managed;
    public Animator sceneTransition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keybindManager = GetComponent<KeybindManager>();
    }

    public void StartGame()
    {
        PlayExitTransition();
    }

    private void PlayExitTransition()
    {
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

        Managed.SetActive(true);

        // Load the new scene
        yield return StartCoroutine(LoadNewScene(startingScene));
    }

    private void PlayEnterTransition()
    {
        sceneTransition.Play("enterScene");
    }

    private IEnumerator LoadNewScene(string newScene)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Single);
        yield return loadOp;

        // Wait 1 frame to ensure everything is initialized
        yield return null;

        PlayEnterTransition();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
