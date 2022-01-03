using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AudioSourcePlayMode
{
    Random,
    Average
}

public static class Utility
{
    /// <summary>
    /// Play a one-shot <see cref="SO_ExtendedClip"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="clip">A pre-configured <see cref="SO_ExtendedClip"/>.</param>
    /// <param name="mode">Based on this mode, the resulting audiobyte may be different.</param>
    public static void PlayOneShot(this AudioSource source, SO_ExtendedClip clip, AudioSourcePlayMode mode)
    {
        switch (mode)
        {
            case AudioSourcePlayMode.Random:
                source.pitch = clip.Pitch.Random;
                source.PlayOneShot(clip.AudioClip, clip.Volume.Random);
                break;

            case AudioSourcePlayMode.Average:
                source.pitch = clip.Pitch.Average;
                source.PlayOneShot(clip.AudioClip, clip.Volume.Average);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}
