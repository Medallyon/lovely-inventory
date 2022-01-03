using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    [Header("Essential")]

    [Required] public TextMeshProUGUI TextElement;

    [Tooltip("Whether the Typewriter is enabled. If disabled, simply replaces the text without any special effects.")]
    public bool Enabled = true;

    [Header("Typewriter Settings")]

    [Tooltip("Whether the typing speed should be based on the length of the string.")]
    [ShowIf(nameof(Enabled))] public bool Dynamic;

    [Tooltip("Time for each letter to appear, in Seconds")]
    [ShowIf(nameof(Enabled))] [DisableIf(nameof(Dynamic))] public float TypingSpeed = .025f;

    [Tooltip("Play a clip every time a letter is generated.")]
    [ShowIf(nameof(Enabled))] public SO_ExtendedClip TypingClip;

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            _text = value;

            if (!Enabled)
            {
                TextElement.text = value;
                return;
            }

            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(SetText(value));
        }
    }

    private Coroutine _activeCoroutine;
    private bool _isAudioClipNotNull;
    private AudioSource _audio;

    private void Start()
    {
        _isAudioClipNotNull = TypingClip != null && TypingClip.AudioClip != null;

        _audio = GetComponent<AudioSource>();
        if (_isAudioClipNotNull && !_audio)
            _audio = gameObject.AddComponent<AudioSource>();
    }

    private IEnumerator SetText(string text)
    {
        TextElement.text = "";

        // Dynamic formula: {total # of secs for whole string to be typed out} / {text.Length}
        WaitForSeconds wait = new WaitForSeconds(Dynamic ? .5f / text.Length : TypingSpeed);
        foreach (char letter in text)
        {
            // Append each letter consecutively
            TextElement.text = $"{TextElement.text}{letter}";

            // Play a typing sound if available
            if (_isAudioClipNotNull)
                _audio.PlayOneShot(TypingClip, AudioSourcePlayMode.Random);

            // Skip waiting on spaces
            if (letter != ' ')
                yield return wait;
        }

        _activeCoroutine = null;
    }
}
