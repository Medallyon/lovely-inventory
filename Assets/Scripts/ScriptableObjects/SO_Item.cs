using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory Item")]
public class SO_Item : ScriptableObject
{
    [Header("Essential")]

    [Required] public string Name;
    [Required] public Sprite Sprite;

    [Header("Optional")]

    public SO_ExtendedClip PickupClip;
    public SO_ExtendedClip PutDownClip;

    public override string ToString()
    {
        return Name;
    }
}
