using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public sealed class InventoryManager : MonoBehaviour
{
    public InventorySlot this[int index] => ItemGrid.GetChild(index).GetComponent<InventorySlot>();

    [Required]
    public GameObject SelectedMarker;

    [Required]
    public Transform ItemGrid;

    [Required]
    public TextMeshProUGUI ActiveItemText;

    public ParticleSystem Particles;

    [FoldoutGroup("Audio")] public SO_ExtendedClip NavigateClip;
    [FoldoutGroup("Audio")] public SO_ExtendedClip SelectClip;
    [FoldoutGroup("Audio")] public SO_ExtendedClip ShuffleClip;

    public List<SO_Item> AvailableItems = new List<SO_Item>();

    public List<InventorySlot> ItemSlots
    {
        get
        {
            var slots = new List<InventorySlot>();
            for (int i = ItemGrid.childCount; i-- > 0;)
                slots.Add(ItemGrid.GetChild(i).GetComponent<InventorySlot>());
            return slots;
        }
    }

    public string CurrentItemName
    {
        set => ActiveItemText.text = value;
    }

    private InventorySlot CurrentSlot => ItemGrid.GetChild(CurrentIndex).GetComponent<InventorySlot>();

    private int _index;
    private int CurrentIndex
    {
        get => _index;
        set
        {
            _index = Mathf.Clamp(value, 0, ItemGrid.childCount - 1);
            SelectedMarker.transform.SetParent(ItemGrid.GetChild(_index), false);
            CurrentItemName = CurrentSlot.Item != null ? CurrentSlot.Item.Name : "";
        }
    }

    private int PreviousIndex { get; set; }

    [Header("Buttons")]

    private int _selectedIndex = -1;
    private SO_Item _selectedItem = null;
    private SO_Item _inHolding = null;
    
    private AudioSource Audio { get; set; }
    private readonly System.Random _rnd = new System.Random();

    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ShuffleItems();
    }

    [Button]
    private void SetRandomSelection()
    {
        CurrentIndex = Random.Range(0, ItemGrid.childCount);
    }

    public void Select()
    {
        if (_selectedIndex == -1 && CurrentSlot.Item != null)
        { // Pick up the Item on the current Slot
            CurrentSlot.Selected = !CurrentSlot.Selected;
            _selectedIndex = CurrentSlot.Selected ? CurrentIndex : -1;
            _selectedItem = CurrentSlot.Item;
            _inHolding = _selectedItem;

            { // Play a simple animation to convey that the desired Item is in the "picked up" state
                this[_selectedIndex].GetComponent<Image>().CrossFadeColor(new Color(1, 0.79f, 0.79f), .5f, false, false);
                this[_selectedIndex].ChildImage.CrossFadeAlpha(.5f, 1f, false);

                iTween.ScaleTo(this[_selectedIndex].ChildImage.gameObject, iTween.Hash(
                    "name", "scale_vibrate",
                    "scale", new Vector3(.5f, .5f, .5f),
                    "time", .5f,
                    "easetype", iTween.EaseType.easeInOutCubic,
                    "looptype", iTween.LoopType.pingPong
                ));
            }

            { // Play this clip in reverse (I felt this sounded nicer the other way around)
                Audio.clip = SelectClip.AudioClip;
                Audio.pitch = -SelectClip.Pitch.Random;
                Audio.timeSamples = SelectClip.AudioClip.samples - 1;
                Audio.Play();
            }
        }

        else if (_selectedIndex > -1)
        { // Reset Selection
            this[_selectedIndex].Item = _inHolding;

            { // Audio for Putting the Item down
                Audio.pitch = SelectClip.Pitch.Random;
                Audio.PlayOneShot(SelectClip.AudioClip);
            }

            SpawnParticles();
            ResetInventory();
        }
    }

    public void Navigate(string controlName)
    {
        PreviousIndex = CurrentIndex;
        controlName = controlName.ToLower();

        int calculatedIndex = CurrentIndex;
        if (controlName.Contains("up"))
            calculatedIndex = CurrentIndex < 6 ? CurrentIndex : CurrentIndex - 6;
        else if (controlName.Contains("down"))
            calculatedIndex = CurrentIndex > 11 ? CurrentIndex : CurrentIndex + 6;
        else if (controlName.Contains("left"))
            calculatedIndex--;
        else if (controlName.Contains("right"))
            calculatedIndex++;
        calculatedIndex = Mathf.Clamp(calculatedIndex, 0, ItemGrid.childCount - 1);

        if (CurrentIndex == calculatedIndex)
            return;

        if (_selectedIndex > -1)
        {
            InventorySlot oldSlot = this[_selectedIndex];
            InventorySlot newSlot = this[calculatedIndex];
            InventorySlot previousSlot = this[PreviousIndex];

            previousSlot.Item = _inHolding;

            _inHolding = calculatedIndex == _selectedIndex ? _selectedItem : newSlot.Item;
            if (_inHolding != null)
                oldSlot.Spawn(_inHolding);
            else if (newSlot.Item == null)
                oldSlot.Item = _selectedItem;

            newSlot.Item = _selectedItem;

            if (calculatedIndex != CurrentIndex)
            {
                newSlot.Selected = true;
                CurrentSlot.Selected = false;
            }
        }

        CurrentIndex = calculatedIndex;

        { // Audio for Navigating the Inventory
            Audio.pitch = NavigateClip.Pitch.Random;
            Audio.PlayOneShot(NavigateClip.AudioClip);
        }
    }

    [Button]
    public void ShuffleItems() => ShuffleItems(5);
    public void ShuffleItems(int amount)
    {
        // Clean up data in case we were in 'selection' mode
        ResetInventory();

        { // Audio for Putting the Item down
            Audio.pitch = ShuffleClip.Pitch.Random;
            Audio.PlayOneShot(ShuffleClip.AudioClip);
        }

        amount = Mathf.Clamp(amount, 0, ItemGrid.childCount);

        // Store this so that the expensive Getter isn't accessed over and over
        var slots = ItemSlots;

        // Clear every item slot first
        foreach (InventorySlot slot in slots)
            slot.Item = null;

        // Evently select X slots and populate them with a random Item using a simple Linq expression
        foreach (InventorySlot item in slots.OrderBy(x => _rnd.Next()).Take(amount))
            item.Spawn(AvailableItems[Random.Range(0, AvailableItems.Count)]);

        // Alternatively, use a less elegant approach that implements some form of Selection Sampling:

        /*
        // Store this so that the expensive Getter isn't accessed over and over
        var slots = ItemSlots;

        // Clear every item slot first
        foreach (InventoryItem slot in slots)
            slot.Item = null;

        // Create a list to hold the randomly selected Items
        var selected = new List<InventoryItem>();
        for (int i = 0; i < amount; i++)
            selected.Add(slots[i]);

        for (int i = amount - 1; i < ItemGrid.childCount; i++)
        {
            // Pick a random index ranging 0..i+1
            int j = _rnd.Next(i + 1);

            // If the random index is less than our amount (default:5), overwrite it in the output list
            if (j < amount)
                selected[j] = slots[i];
        }

        // Assign a random item to the selected slots
        foreach (InventoryItem item in selected)
            item.Spawn(AvailableItems[Random.Range(0, AvailableItems.Count)]);
        */

        // Don't forget to call the setter to update the Item Text
        CurrentIndex = CurrentIndex;
    }

    private void ResetInventory()
    {
        if (_selectedIndex > -1)
        {
            this[_selectedIndex].GetComponent<Image>().CrossFadeColor(new Color(1f, 1f, 1f), .25f, false, false);
            this[_selectedIndex].ChildImage.CrossFadeAlpha(1f, .5f, false);

            iTween.StopByName(this[_selectedIndex].ChildImage.gameObject, "scale_vibrate");
            this[_selectedIndex].ChildImage.transform.localScale = new Vector3(.7f, .7f, .7f);
        }

        _selectedIndex = -1;
        _selectedItem = null;
        _inHolding = null;

        CurrentSlot.Selected = false;
    }

    private ParticleSystem SpawnParticles()
    {
        ParticleSystem particles = Instantiate(Particles, CurrentSlot.ChildImage.transform, true);

        { // Adjust world position
            Transform trans = particles.transform;
            trans.localPosition = new Vector3(0, 0, -100);
            trans.localScale = Vector3.one;
        }

        { // Calculate matching colors of the current Item and set them as the Particle System's startColor
            var thief = new ColorThief.ColorThief();
            var palette = thief.GetPalette((Texture2D) CurrentSlot.ChildImage.mainTexture);
            var gradient = new ParticleSystem.MinMaxGradient(palette[Random.Range(0, palette.Count)].UnityColor,
                palette[Random.Range(0, palette.Count)].UnityColor);

            var main = particles.main;
            main.startColor = gradient;
        }

        return particles;
    }
}
