using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public InventorySlot this[int index] => ItemGrid.GetChild(index).GetComponent<InventorySlot>();

    [Required]
    public GameObject SelectedMarker;

    [Required]
    public Transform ItemGrid;

    [Required]
    public TextMeshProUGUI ActiveItemText;

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

    private int _selectedIndex = -1;
    private SO_Item _selectedItem = null;
    private SO_Item _inHolding = null;

    [Header("Buttons")]

    private readonly System.Random _rnd = new System.Random();

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
        if (CurrentSlot.Item != null)
        {
            CurrentSlot.Selected = !CurrentSlot.Selected;
            _selectedIndex = CurrentSlot.Selected ? CurrentIndex : -1;
            _selectedItem = CurrentSlot.Item;
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

        if (_selectedIndex > -1)
        {
            InventorySlot oldSlot = this[_selectedIndex];
            InventorySlot newSlot = this[calculatedIndex];
            InventorySlot previousSlot = this[PreviousIndex];

            previousSlot.Item = _inHolding;

            _inHolding = newSlot.Item;
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
    }

    [Button]
    public void ShuffleItems() => ShuffleItems(5);
    public void ShuffleItems(int amount)
    {
        { // Clean up data in case we were in 'selection' mode
            _selectedIndex = -1;
            _selectedItem = null;
            _inHolding = null;

            CurrentSlot.Selected = false;
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
}
