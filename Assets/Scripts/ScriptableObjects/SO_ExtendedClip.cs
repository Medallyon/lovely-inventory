using System;
using UnityEngine;

public interface IRanges
{
    public float Random { get; }
    public float Average { get; }
}

[Serializable]
public struct VolumeRange : IRanges
{
    [Range(0f, 10f)] public float Min;
    [Range(0f, 10f)] public float Max;

    public float Random => UnityEngine.Random.Range(Min, Max);
    public float Average => (Min + Max) / 2;
}

[Serializable]
public struct PitchRange : IRanges
{
    [Range(-10f, 10f)] public float Min;
    [Range(-10f, 10f)] public float Max;

    public float Random => UnityEngine.Random.Range(Min, Max);
    public float Average => (Min + Max) / 2;
}

[CreateAssetMenu(fileName = "Clip_", menuName = "Extended Audio Clip")]
public class SO_ExtendedClip : ScriptableObject
{
    public AudioClip AudioClip;
    public VolumeRange Volume;
    public PitchRange Pitch;

    private void OnValidate()
    {
        { // Volume
            if (Volume.Min > Volume.Max)
                Volume.Max = Volume.Min;
            else if (Volume.Max < Volume.Min)
                Volume.Min = Volume.Max;
        }

        { // Pitch
            if (Pitch.Min > Pitch.Max)
                Pitch.Min = Pitch.Max;
            else if (Pitch.Max < Pitch.Min)
                Pitch.Max = Pitch.Min;
        }
    }

    private void Reset()
    {
        Volume.Min = 1f;
        Volume.Max = 1f;

        Pitch.Min = .95f;
        Pitch.Max = 1.05f;
    }
}
