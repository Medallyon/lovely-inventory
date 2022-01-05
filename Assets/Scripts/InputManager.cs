using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [Required] public InventoryManager InventoryManager;
    [Required] public ResolutionManager ResolutionManager;

    protected Actions_Default Actions { get; set; }

    #region Unity Methods

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
        { // Set up any input actions
            Actions = new Actions_Default();

            Actions.UI.Navigate.performed += Navigate;
            Actions.UI.Select.performed += Select;
            Actions.UI.Shuffle.performed += Shuffle;
            Actions.UI.CycleResolution.performed += CycleResolution;
            Actions.UI.Quit.performed += Quit;
        }
    }

    #endregion Unity Methods
    #region Player Input Methods

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

    protected void CycleResolution(InputAction.CallbackContext context) => ResolutionManager.CycleResolution(context);

    protected static void Quit(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    #endregion Player Input Methods
}
