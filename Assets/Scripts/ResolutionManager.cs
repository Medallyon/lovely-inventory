using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    [Required] public CanvasScaler CanvasScaler;

    public SO_ExtendedClip ResolutionClip;

    protected AudioSource Audio { get; set; }

    private bool _wasFullScreen;
    protected Action OnFullScreen;
    protected Action OnWindowed;

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
            CurrentResolutionIndex = AvailableResolutions.FindIndex(res => res.width == value.width);

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

    protected void OnValidate()
    {
        CanvasScaler.referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        if (GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();
    }

    protected void Awake()
    {
        Audio = GetComponent<AudioSource>();

        { // Handle fullscreen logic here
            _wasFullScreen = !Screen.fullScreen;

            { // Remove any resolutions not supported by the current Display
                var screenResolutions = new List<Resolution>(Screen.resolutions);
                for (int i = AvailableResolutions.Count; i-- > 0;)
                {
                    if (screenResolutions.FindIndex(res => res.width == AvailableResolutions[i].width) == -1)
                        AvailableResolutions.RemoveAt(i);
                }

                CurrentResolutionIndex = AvailableResolutions.FindIndex(res => res.width == Screen.currentResolution.width);
                if (CurrentResolutionIndex == -1)
                    CurrentResolution = AvailableResolutions[AvailableResolutions.Count - 1];
            }

            OnWindowed += () => { CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; };
            OnFullScreen += () =>
            {
                CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                CurrentResolution = CurrentResolution;
            };
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

    public void CycleResolution(InputAction.CallbackContext context)
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
                Audio.PlayOneShot(ResolutionClip, AudioSourcePlayMode.Random);
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
                Audio.PlayOneShot(ResolutionClip, AudioSourcePlayMode.Random);
            }

            CurrentResolutionIndex++;
        }

        Debug.Log($"Setting Resolution to {AvailableResolutions[CurrentResolutionIndex]}");
        CurrentResolution = AvailableResolutions[CurrentResolutionIndex];
    }
}
