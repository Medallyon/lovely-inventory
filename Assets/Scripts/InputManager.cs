using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Required]
    public InventoryManager InventoryManager;

    protected Actions_Default Actions { get; set; }

    private void OnEnable()
    {
        Actions.Enable();
    }

    private void OnDisable()
    {
        Actions.Disable();
    }

    protected void Awake()
    {
        Actions = new Actions_Default();

        Actions.UI.Navigate.performed += Navigate;
        Actions.UI.Select.performed += Select;
        Actions.UI.Shuffle.performed += Shuffle;
        Actions.UI.CycleResolution.performed += CycleResolution;
        Actions.UI.Quit.performed += Quit;
    }

    protected void Navigate(InputAction.CallbackContext context)
    {
        Vector2 val = context.ReadValue<Vector2>();
        string dir = string.Empty;

        if (val.x >= .7f)
            dir = "right";
        else if (val.x <= -.7f)
            dir = "left";
        else if (val.y >= .7f)
            dir = "up";
        else if (val.y <= -.7f)
            dir = "down";

        InventoryManager.Navigate(dir);
    }

    protected void Select(InputAction.CallbackContext context)
    {
        InventoryManager.Select();
    }

    protected void Shuffle(InputAction.CallbackContext context)
    {
        InventoryManager.ShuffleItems();
    }

    protected List<Resolution> AvailableResolutions { get; } = new List<Resolution>
    {
        new Resolution() { width = 1280, height = 720 },
        new Resolution() { width = 1920, height = 1080 },
        new Resolution() { width = 3860, height = 2160 }
    };

    protected Resolution CurrentResolution
    {
        get => AvailableResolutions[CurrentResolutionIndex];
        set => Screen.SetResolution(value.width, value.height, Screen.fullScreen);
    }

    private int _currentResolutionIndex = 1;
    protected int CurrentResolutionIndex
    {
        get => _currentResolutionIndex;
        set => _currentResolutionIndex = Math.Min(AvailableResolutions.Count, Math.Max(0, value));
    }

    protected void CycleResolution(InputAction.CallbackContext context)
    {
        Debug.Log(context.control.name);

        // Player wants to lower resolution
        if (context.control.name.ToLower().Contains("left"))
        {
            // Resolution is already the lowest available
            if (CurrentResolutionIndex == 0)
                return;

            // Check that monitor supports target resolution
            if ((new List<Resolution>(Screen.resolutions)).FindIndex(res =>
                res.width == AvailableResolutions[CurrentResolutionIndex - 1].width) == -1)
            {
                Debug.LogWarning($"This display does not support {AvailableResolutions[CurrentResolutionIndex - 1]}.");
                return;
            }

            CurrentResolutionIndex--;
        }

        // Player wants to increase resolution
        else if (context.control.name.ToLower().Contains("right"))
        {
            // Resolution is already the highest available
            if (CurrentResolutionIndex == AvailableResolutions.Count - 1)
                return;

            // Check that monitor supports target resolution
            if ((new List<Resolution>(Screen.resolutions)).FindIndex(res =>
                res.width == AvailableResolutions[CurrentResolutionIndex + 1].width) == -1)
            {
                Debug.LogWarning($"This display does not support {AvailableResolutions[CurrentResolutionIndex + 1]}.");
                return;
            }

            CurrentResolutionIndex++;
        }

        Debug.Log($"Setting Resolution to {AvailableResolutions[CurrentResolutionIndex]}");
        CurrentResolution = AvailableResolutions[CurrentResolutionIndex];
    }

    private void Quit(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
