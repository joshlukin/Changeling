using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    public bool HasItems() => items.Count > 0;

    // Returns and removes the first item, or null if empty
    public Item TakeFirstItem()
    {
        if (items.Count == 0) return null;
        Item item = items[0];
        items.RemoveAt(0);
        return item;
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }
}