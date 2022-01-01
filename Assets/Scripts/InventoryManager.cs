using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Required]
    public GameObject SelectedMarker;

    [Required]
    public Transform ItemGrid;

    public List<SO_Item> AvailableItems = new List<SO_Item>();

    private int _selected;
    protected int SelectedIndex
    {
        get => _selected;
        set
        {
            _selected = Mathf.Min(ItemGrid.childCount - 1, Mathf.Max(0, value));
            SelectedMarker.transform.position = ItemGrid.GetChild(_selected).transform.position - new Vector3(9.2f, -9.2f, 0);
        }
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

    [Button] private void NavLeft() => Navigate("left");
    [Button] private void NavRight() => Navigate("right");
    [Button] private void NavUp() => Navigate("up");
    [Button] private void NavDown() => Navigate("down");
}
