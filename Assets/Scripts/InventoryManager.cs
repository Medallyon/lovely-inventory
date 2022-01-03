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

    [Header("Essential")]

    [Tooltip("This is the GameObject that indicates which Item Slot the player is on.")]
    [Required] public GameObject SelectedMarker; // Also referred to as the "Cursor" throughout this document

    [Tooltip("Drag in the parent Transform that contains all Item Slots.")]
    [Required] public Transform ItemGrid;

    [Tooltip("A Typewriter instance. This represents the text element that reflects which Item Slot the player is on.")]
    [Required] public Typewriter ActiveItemText;

    [Header("Optional")]

    [Tooltip("A Particle System that is played when the Player has successfully put an Item back down.")]
    public ParticleSystem Particles;

    [Tooltip("A clip that plays when the Player moves the cursor.")]
    [FoldoutGroup("Audio")] public SO_ExtendedClip NavigateClip;

    [Tooltip("A clip that plays when the Player picks up an Item.")]
    [FoldoutGroup("Audio")] public SO_ExtendedClip SelectClip;

    [Tooltip("A clip that plays when the Player shuffles the Inventory.")]
    [FoldoutGroup("Audio")] public SO_ExtendedClip ShuffleClip;

    [Tooltip("Any ItemData that may be chosen at random when the Inventory is shuffled.")]
    public List<SO_Item> AvailableItems = new List<SO_Item>();

    /// <summary>
    /// A rather expensive Getter property that collects all Slots and casts them accordingly.
    /// </summary>
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

    private int _index;
    /// <summary>
    /// The current index of the <see cref="InventorySlot"/> under the cursor based on its position in the Inventory.
    /// </summary>
    private int CurrentIndex
    {
        get => _index;
        set
        {
            _index = Mathf.Clamp(value, 0, ItemGrid.childCount - 1);

            // Move the cursor to the desired position
            SelectedMarker.transform.SetParent(ItemGrid.GetChild(_index), false);

            // Update the Item Name Text value, as long as we're not in 'pickup' mode
            if (_selectedIndex == -1)
                ActiveItemText.Text = CurrentSlot.Item != null ? CurrentSlot.Item.Name : "";
        }
    }

    /// <summary>
    /// A shorthand for the <see cref="InventorySlot"/> under the cursor.
    /// </summary>
    private InventorySlot CurrentSlot => this[CurrentIndex];

    private int _selectedIndex = -1;
    private int _previousIndex = -1;
    private SO_Item _selectedItem;
    private SO_Item _inHolding;
    
    private AudioSource Audio { get; set; }
    private readonly System.Random _rnd = new System.Random();

    #region Unity Methods

    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ShuffleItems();
        CurrentIndex = 0;
    }

    #endregion Unity Methods
    #region Public Methods

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

            // Audio for Putting the Item down
            Audio.PlayOneShot(SelectClip, AudioSourcePlayMode.Random);

            SpawnParticles();
            ResetInventory();
        }
    }

    /// <summary>
    /// Navigate the cursor into the desired direction based on the <paramref name="controlName"/>.
    /// </summary>
    /// <param name="controlName">A choice of either Up, Down, Left, or Right.</param>
    public void Navigate(string controlName)
    {
        _previousIndex = CurrentIndex;
        controlName = controlName.ToLower();

        int calculatedIndex = CurrentIndex;

        { // Movement logic - this is pre-calculated because we may not want to move the cursor yet.
            if (controlName.Contains("up"))
                calculatedIndex = CurrentIndex < 6 ? CurrentIndex : CurrentIndex - 6;
            else if (controlName.Contains("down"))
                calculatedIndex = CurrentIndex > 11 ? CurrentIndex : CurrentIndex + 6;
            else if (controlName.Contains("left"))
                calculatedIndex--;
            else if (controlName.Contains("right"))
                calculatedIndex++;
            calculatedIndex = Mathf.Clamp(calculatedIndex, 0, ItemGrid.childCount - 1);
        }

        // The player is trying to move out of bounds. Stop that.
        if (CurrentIndex == calculatedIndex)
            return;

        if (_selectedIndex > -1)
        { // We are in 'pickup' mode
            InventorySlot oldSlot = this[_selectedIndex];
            InventorySlot newSlot = this[calculatedIndex];
            InventorySlot previousSlot = this[_previousIndex];

            previousSlot.Item = _inHolding;

            // Special check to ensure that the selected item won't be duplicated
            _inHolding = calculatedIndex == _selectedIndex ? _selectedItem : newSlot.Item;
            if (_inHolding != null)
                oldSlot.Spawn(_inHolding);
            else if (newSlot.Item == null)
                oldSlot.Item = _selectedItem;

            // Set the new slot to contain the item that the Player originally 'picked up'
            newSlot.Item = _selectedItem;

            // Play the 'pickup' animation on the new slot
            newSlot.Selected = true;
            CurrentSlot.Selected = false;
        }

        // Update 'CurrentIndex' with the slot index that we should be on
        CurrentIndex = calculatedIndex;

        // Audio for Navigating the Inventory
        Audio.PlayOneShot(NavigateClip, AudioSourcePlayMode.Random);
    }

    /// <summary>
    /// Clear the Inventory and replace it with 5 randomly selected Items.
    /// </summary>
    public void ShuffleItems() => ShuffleItems(5);
    /// <summary>
    /// Clear the Inventory and replace it with <paramref name="amount"/> randomly selected Items.
    /// </summary>
    /// <param name="amount">The amount of new <see cref="InventorySlot"/>s we should populate.</param>
    public void ShuffleItems(int amount)
    {
        // Clear the board and clean up data in case we were in 'selection' mode
        ResetInventory();

        // Audio for Putting the Item down
        Audio.PlayOneShot(ShuffleClip, AudioSourcePlayMode.Random);

        amount = Mathf.Clamp(amount, 0, ItemGrid.childCount);

        {
            // Store this so that the expensive Getter isn't accessed over and over
            var slots = ItemSlots;

            // Clear every item slot first
            foreach (InventorySlot slot in slots)
                slot.Item = null;

            // Evently select X slots and populate them with a random Item using a simple Linq expression
            foreach (InventorySlot item in slots.OrderBy(x => _rnd.Next()).Take(amount))
                item.Spawn(AvailableItems[Random.Range(0, AvailableItems.Count)]);
        }

        // Alternatively, use a less elegant approach that implements some form of Selection Sampling:

        /*{
            // Store this so that the expensive Getter isn't accessed over and over
            var slots = ItemSlots;

            // Clear every item slot first
            foreach (InventorySlot slot in slots)
                slot.Item = null;

            // Create a list to hold the randomly selected Items
            var selected = new List<InventorySlot>();
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
            foreach (InventorySlot item in selected)
                item.Spawn(AvailableItems[Random.Range(0, AvailableItems.Count)]);
        }*/

        // Don't forget to call the setter to update the Item Text
        CurrentIndex = CurrentIndex;
    }

    #endregion Public Methods
    #region Private Methods

    private void ResetInventory()
    {
        if (_selectedIndex > -1)
        { // Return the selected InventorySlot to normal and stop any ongoing animations
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

    #endregion Private Methods
}
