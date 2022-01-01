using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory Item")]
public class SO_Item : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
}
