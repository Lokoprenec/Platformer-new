using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;

public class ControlRemapping : MonoBehaviour
{
    private KeybindManager kM;
    public bool isRemapping;
    public Controls controlToRemap;
    public List<Button> buttons;
    public int buttonIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kM = FindAnyObjectByType<KeybindManager>();

        // Get all controls in enum order
        Controls[] controls = (Controls[])Enum.GetValues(typeof(Controls));

        // Loop through and assign listeners
        for (int i = 0; i < buttons.Count && i < controls.Length; i++)
        {
            buttonIndex = i;
            Controls control = controls[i];
            Button button = buttons[i];
            int index = i;

            // Capture the enum value correctly for this iteration
            button.onClick.AddListener(() => StartRemap(control, index));

            FieldInfo field = typeof(KeybindManager).GetField(control.ToString());
            KeyCode keyValue = (KeyCode)field.GetValue(kM);
            UpdateUI(control.ToString(), keyValue.ToString());
        }
    }

    void Update()
    {
        if (isRemapping)
        {
            DetectKeyPress();
        }
    }

    public void StartRemap(Controls control, int index)
    {
        buttonIndex = index;
        Button selectedButton = buttons[buttonIndex];
        TextMeshProUGUI buttonText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "...";
        controlToRemap = control;
        isRemapping = true;
    }

    void DetectKeyPress()
    {
        // Loop through all KeyCodes to detect which one was pressed
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                Remap(controlToRemap, keyCode);
                break; // stop checking once we find one key
            }
        }
    }

    void Remap(Controls control, KeyCode key)
    {
        // Use reflection to find the matching field by name
        FieldInfo field = typeof(KeybindManager).GetField(control.ToString());
        field.SetValue(kM, key);
        KeyCode keyValue = (KeyCode)field.GetValue(kM);
        UpdateUI(control.ToString(), keyValue.ToString());
        isRemapping = false;
    }

    void UpdateUI(string controlName, string keyCode)
    {
        Button selectedButton = buttons[buttonIndex];
        TextMeshProUGUI controlText = selectedButton.transform.parent.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI buttonText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
        controlText.text = controlName;
        buttonText.text = keyCode;
    }
}
