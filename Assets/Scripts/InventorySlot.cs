using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private SO_Item _item;
    private Image _childImage;

    private bool _selected;
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            if (!value)
            {
                ChildImage.transform.localRotation = Quaternion.identity;
                iTween.StopByName(ChildImage.gameObject, "selected");
            }

            else
            {
                ChildImage.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
                iTween.RotateAdd(ChildImage.gameObject, iTween.Hash(
                    "name", "selected",
                    "y", 90f,
                    "time", 1f,
                    "easetype", iTween.EaseType.easeInOutSine,
                    "looptype", iTween.LoopType.pingPong
                ));
            }
        }
    }

    public Image ChildImage => _childImage != null ? _childImage : (transform.childCount == 0
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

    public void Spawn(SO_Item item)
    {
        Item = item;
        PlaySpawnAnimation();
    }

    private void PlaySpawnAnimation()
    {
        if (!Application.isPlaying)
            return;

        ChildImage.transform.localScale = new Vector3(.001f, .001f, .001f);

        if (iTween.tweens.Find(x => (string)x["name"] == "scale_vibrate") != null)
            return;

        iTween.StopByName(ChildImage.gameObject, "spawn");
        iTween.ScaleTo(ChildImage.gameObject, iTween.Hash(
            "name", "spawn",
            "scale", new Vector3(.7f, .7f, .7f),
            "delay", Random.Range(0f, .15f),
            "time", 1,
            "easetype", iTween.EaseType.easeOutElastic
        ));
    }

    public override string ToString()
    {
        return Item != null ? Item.Name : base.ToString();
    }
}
