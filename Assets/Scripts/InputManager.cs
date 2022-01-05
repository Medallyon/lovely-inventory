using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [Required] public InventoryManager InventoryManager;
    [Required] public CanvasScaler CanvasScaler;

    public SO_ExtendedClip ResolutionClip;

    private bool _wasFullScreen;
    protected Action OnFullScreen;
    protected Action OnWindowed;

    protected Actions_Default Actions { get; set; }

    protected List<Resolution> AvailableResolutions { get; } = new List<Resolution>
    {
        new Resolution() { width = 1280, height = 720 },
        new Resolution() { width = 1920, height = 1080 },
        new Resolution() { width = 3860, height = 2160 }
    };

    protected Resolution CurrentResolution
    {
        get => AvailableResolutions[CurrentResolutionIndex];
        set
        {
            Screen.SetResolution(value.width, value.height, Screen.fullScreen);

            if (!Screen.fullScreen)
                return;

            // In FullScreen Mode, Scale Mode will be 'Constant Pixel Size'. This allows us to modify the canvas scale manually, based on the current resolution and the maximum available resolution that this Display can handle. This is in an attempt to mimic the process that is applied when switching resolutions in Windowed Mode, where seemingly no quality is lost. There may be a better way.

            float min = AvailableResolutions[0].width;
            float max = AvailableResolutions[AvailableResolutions.Count - 1].width;
            // Normalize scale factor into a value between 25%..100%
            CanvasScaler.scaleFactor = Mathf.Max(.25f, (value.width - min) / (max - min));
        }
    }

    private int _currentResolutionIndex;
    protected int CurrentResolutionIndex
    {
        get => _currentResolutionIndex;
        set => _currentResolutionIndex = Math.Min(AvailableResolutions.Count, Math.Max(0, value));
    }

    private void OnEnable()
    {
        Actions.Enable();
    }

    private void OnDisable()
    {
        Actions.Disable();
    }

    protected void OnValidate()
    {
        CanvasScaler.referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
    }

    protected void Awake()
    {
        { // Handle fullscreen logic here
            _wasFullScreen = !Screen.fullScreen;

            { // Remove any resolutions not supported by the current Display
                var screenResolutions = new List<Resolution>(Screen.resolutions);
                for (int i = AvailableResolutions.Count; i-- > 0;)
                {
                    if (screenResolutions.FindIndex(res => res.width == AvailableResolutions[i].width) == -1)
                        AvailableResolutions.RemoveAt(i);
                }

                CurrentResolutionIndex = AvailableResolutions.Count - 1;
            }

            OnWindowed += () => { CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; };
            OnFullScreen += () =>
            {
                CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                CurrentResolution = CurrentResolution;
            };
        }

        { // Set up any input actions
            Actions = new Actions_Default();

            Actions.UI.Navigate.performed += Navigate;
            Actions.UI.Select.performed += Select;
            Actions.UI.Shuffle.performed += Shuffle;
            Actions.UI.CycleResolution.performed += CycleResolution;
            Actions.UI.Quit.performed += Quit;
        }
    }

    protected void Update()
    {
        { // Wrapper for the poor man's FullScreen Event as Unity doesn't provide stuff like this natively
            if (Screen.fullScreen && !_wasFullScreen)
            {
                _wasFullScreen = !_wasFullScreen;
                OnFullScreen.Invoke();
            }

            else if (!Screen.fullScreen && _wasFullScreen)
            {
                _wasFullScreen = !_wasFullScreen;
                OnWindowed.Invoke();
            }
        }
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

    protected void CycleResolution(InputAction.CallbackContext context)
    {
        // Player wants to lower resolution
        if (context.control.name.ToLower().Contains("left") || context.control.name.ToLower().Contains("q"))
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

            if (ResolutionClip != null)
            {
                ResolutionClip.Pitch = new PitchRange { Min = .9f, Max = .9f };
                InventoryManager.GetComponent<AudioSource>().PlayOneShot(ResolutionClip, AudioSourcePlayMode.Random);
            }

            CurrentResolutionIndex--;
        }

        // Player wants to increase resolution
        else if (context.control.name.ToLower().Contains("right") || context.control.name.ToLower().Contains("e"))
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

            if (ResolutionClip != null)
            {
                ResolutionClip.Pitch = new PitchRange { Min = 1.1f, Max = 1.1f };
                InventoryManager.GetComponent<AudioSource>().PlayOneShot(ResolutionClip, AudioSourcePlayMode.Random);
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
