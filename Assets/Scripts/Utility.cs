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
    public static void Play(this AudioSource source, SO_ExtendedClip clip)
    {
        source.pitch = clip.Pitch.Random;
        source.PlayOneShot(clip.AudioClip, clip.Volume.Random);
    }
    public static void Play(this AudioSource source, SO_ExtendedClip clip, AudioSourcePlayMode mode)
    {
        switch (mode)
        {
            case AudioSourcePlayMode.Random:
                source.Play(clip);
                break;
            case AudioSourcePlayMode.Average:
                source.pitch = clip.Pitch.Average;
                source.PlayOneShot(clip.AudioClip, clip.Volume.Average);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public static void SetAlpha(this Graphic graphic, float alpha)
    {
        if (alpha > 1f)
            alpha = (alpha - 0f) / (255f - 0f);
        alpha = Mathf.Clamp(alpha, 0f, 1f);

        Color c = graphic.color;
        graphic.color = new Color(c.r, c.g, c.b, alpha);
    }
}
