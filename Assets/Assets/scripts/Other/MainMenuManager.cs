using UnityEngine;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    private KeybindManager keybindManager;
    public Canvas MainMenuCanvas;
    public Canvas OptionsCanvas;
    public Canvas ExitCanvas;
    public Canvas ControlCanvas;
    private List<Canvas> canvases;
    public int preSelectSortOrder;
    public int postSelectSortOrder;
    private string currentOpenCanvas;

    private void Start()
    {
        keybindManager = FindAnyObjectByType<KeybindManager>();
        Cursor.lockState = CursorLockMode.None;
        MainMenuCanvas.sortingOrder = preSelectSortOrder;

        canvases = new List<Canvas>();
        canvases.Add(MainMenuCanvas);
        canvases.Add(OptionsCanvas);
        canvases.Add(ExitCanvas);
        canvases.Add(ControlCanvas);

        ResetToMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(keybindManager.Exit))
        {
            switch (currentOpenCanvas)
            {
                case "options":

                    CloseOptions();

                    break;

                case "exit":

                    CloseExitScreen();

                    break;

                case "controls":

                    CloseControls();

                    break;

                case "main menu":

                    OpenExitScreen();

                    break;
            }
        }
    }

    public void ResetToMainMenu()
    {
        foreach (Canvas canvas in canvases)
        {
            canvas.gameObject.SetActive(false);
        }

        MainMenuCanvas.gameObject.SetActive(true);
        currentOpenCanvas = "main menu";
    }

    public void SelectAndHide()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MainMenuCanvas.sortingOrder = postSelectSortOrder;
    }

    public void CloseOptions()
    {
        MainMenuCanvas.gameObject.SetActive(true);
        OptionsCanvas.gameObject.SetActive(false);
        currentOpenCanvas = "main menu";
    }

    public void OpenOptions()
    {
        MainMenuCanvas.gameObject.SetActive(false);
        OptionsCanvas.gameObject.SetActive(true);
        currentOpenCanvas = "options";
    }

    public void OpenExitScreen()
    {
        MainMenuCanvas.gameObject.SetActive(false);
        ExitCanvas.gameObject.SetActive(true);
        currentOpenCanvas = "exit";
    }

    public void CloseExitScreen()
    {
        MainMenuCanvas.gameObject.SetActive(true);
        ExitCanvas.gameObject.SetActive(false);
        currentOpenCanvas = "main menu";
    }

    public void OpenControls()
    {
        ControlCanvas.gameObject.SetActive(true);
        OptionsCanvas.gameObject.SetActive(false);
        currentOpenCanvas = "controls";
    }

    public void CloseControls()
    {
        ControlCanvas.gameObject.SetActive(false);
        OptionsCanvas.gameObject.SetActive(true);
        currentOpenCanvas = "options";
    }
}
