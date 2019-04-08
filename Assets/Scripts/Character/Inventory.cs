using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] public int ItemCount = 10;
    [SerializeField] public List<InventoryItem> Items = new List<InventoryItem>();
    [SerializeField] public int CurrentlySelected = 0;

    public event EventHandler<InventoryEventArgs> ItemsUpdated;
    public event EventHandler<int> PositionUpdated;

    public void AddItem(BlockTypes blockType)
    {
        foreach (InventoryItem item in Items)
        {
            if (item.BlockType == blockType && item.CanRaiseCount())
            {
                item.RaiseCount();
                ItemsUpdated(this, new InventoryEventArgs());
                return;
            }
        }

        if (Items.Count < 10)
        {
            Items.Add(new InventoryItem(blockType));
            ItemsUpdated(this, new InventoryEventArgs());
        }
    }

    public InventoryItem TakeItem()
    {
        return TakeItem(CurrentlySelected);
    }

    public InventoryItem TakeItem(int index)
    {
        if (Items.Count <= index) return null;

        InventoryItem item = Items[index];

        if (item.GetCount() > 1)
        {
            item.LowerCount();
        }
        else
        {
            Items.RemoveAt(index);
        }

        ItemsUpdated(this, new InventoryEventArgs());

        return item;
    }

    public void Scroll(int delta)
    {
        CurrentlySelected = CurrentlySelected - delta;
        
        if (CurrentlySelected < 0)
        {
            CurrentlySelected = ItemCount + CurrentlySelected;
        }

        if (CurrentlySelected >= ItemCount)
        {
            CurrentlySelected = CurrentlySelected - ItemCount;
        }

        PositionUpdated(this, CurrentlySelected);
        Debug.Log(CurrentlySelected);
    }
}

public class InventoryEventArgs : EventArgs
{
    public InventoryEventArgs() {}
}