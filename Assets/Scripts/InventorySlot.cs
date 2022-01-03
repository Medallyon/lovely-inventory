using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private SO_Item _item;
    private Image _childImage;

    protected Image ChildImage => _childImage != null ? _childImage : (transform.childCount == 0
        ? null
        : transform.GetChild(0).GetComponent<Image>());

    public SO_Item Item
    {
        get => _item;
        set
        {
            _item = value;

            if (transform.childCount == 0)
                return;

            if (Item == null || (Item != null && Item.Sprite == null))
                ChildImage.enabled = false;
            else
            {
                ChildImage.sprite = Item.Sprite;
                ChildImage.enabled = true;

                if (Application.isPlaying)
                    PlaySpawnAnimation();
            }
        }
    }

    private void Awake()
    {
        _childImage = transform.GetChild(0).GetComponent<Image>();
    }

    private void OnValidate()
    {
        // Refresh Child Sprite by accessing the setter on itself
        Item = Item;
    }

    private void PlaySpawnAnimation()
    {
        ChildImage.transform.localScale = new Vector3(.001f, .001f, .001f);

        iTween.StopByName(ChildImage.gameObject, "spawn");
        iTween.ScaleTo(ChildImage.gameObject, iTween.Hash(
            "name", "spawn",
            "scale", new Vector3(.7f, .7f, .7f),
            "delay", Random.Range(0f, .15f),
            "time", 1,
            "easetype", iTween.EaseType.easeOutElastic
        ));
    }
}
