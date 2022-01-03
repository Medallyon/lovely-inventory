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

    [Required]
    public TextMeshProUGUI ActiveItemText;

    public List<SO_Item> AvailableItems = new List<SO_Item>();

    public string SelectedItemName
    {
        set => ActiveItemText.text = value;
    }
    private InventorySlot SelectedItem => ItemGrid.GetChild(SelectedIndex).GetComponent<InventorySlot>();

    private int _selected;
    private int SelectedIndex
    {
        get => _selected;
        set
        {
            _selected = Mathf.Min(ItemGrid.childCount - 1, Mathf.Max(0, value));
            SelectedMarker.transform.SetParent(ItemGrid.GetChild(_selected), false);
            SelectedItemName = SelectedItem.Item != null ? SelectedItem.Item.Name : "";
        }
    }

    [Header("Buttons")]

    private readonly System.Random _rnd = new System.Random();

    private void Start()
    {
        ShuffleItems();
    }

    [Button]
    private void SelectRandom()
    {
        SelectedIndex = Random.Range(0, ItemGrid.childCount);
    }

    public void Navigate(string controlName)
    {
        controlName = controlName.ToLower();
        if (controlName.Contains("up"))
            SelectedIndex = SelectedIndex < 6 ? SelectedIndex : SelectedIndex - 6;
        else if (controlName.Contains("down"))
            SelectedIndex = SelectedIndex > 11 ? SelectedIndex : SelectedIndex + 6;
        else if (controlName.Contains("left"))
            SelectedIndex--;
        else if (controlName.Contains("right"))
            SelectedIndex++;
    }

    [Button]
    public void ShuffleItems() => ShuffleItems(5);
    public void ShuffleItems(int amount)
    {
        amount = Mathf.Clamp(amount, 0, ItemGrid.childCount);

        // Store this so that the expensive Getter isn't accessed over and over
        var slots = ItemSlots;

        // Clear every item slot first
        foreach (InventorySlot slot in slots)
            slot.Item = null;

        // Evently select X slots and populate them with a random Item using a simple Linq expression
        foreach (InventorySlot item in slots.OrderBy(x => _rnd.Next()).Take(amount))
            item.Item = AvailableItems[Random.Range(0, AvailableItems.Count)];

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
            item.Item = AvailableItems[Random.Range(0, AvailableItems.Count)];
        */

        // Don't forget to call the setter to update the Item Text
        SelectedIndex = SelectedIndex;
    }
}
