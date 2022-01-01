using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public bool Selected { get; set; }

    public SO_Item Item;

    private void OnValidate()
    {
        if (transform.childCount == 0)
            return;

        Image target = transform.GetChild(0).GetComponent<Image>();
        if (Item == null || (Item != null && Item.Sprite == null))
            target.enabled = false;
        else
            target.sprite = Item.Sprite;
    }
}
