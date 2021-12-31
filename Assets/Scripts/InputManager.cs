using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    protected Actions_Default Actions { get; set; }

    private void OnEnable()
    {
        Actions.Enable();
    }
    private void OnDisable()
    {
        Actions.Disable();
    }

    void Awake()
    {
        Actions = new Actions_Default();

        Actions.UI.Navigate.performed += Navigate;
        Actions.UI.Select.performed += Select;
        Actions.UI.Shuffle.performed += Shuffle;
        Actions.UI.CycleResolution.performed += CycleResolution;
    }

    protected void Navigate(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    protected void Select(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    protected void Shuffle(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    private void CycleResolution(InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }
}
