using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] public int ItemCount = 10;
    [SerializeField] public InventoryItem[] Items;
    [SerializeField] public int CurrentlySelected = 0;

    public event EventHandler<InventoryEventArgs> ItemsUpdated;
    public event EventHandler<int> PositionUpdated;

    public void Start()
    {
        Items = new InventoryItem[ItemCount];
        Items[0] = new InventoryItem(BlockTypes.BLOCK_TORCH, 20);
    }

    public InventoryItem GetItem(int index)
    {
        return Items[index];
    }

    public bool CanAddItem()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            InventoryItem item = Items[i];
            if (item == null)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(BlockTypes blockType)
    {
        for(int i = 0; i < ItemCount; i++)
        {
            InventoryItem item = Items[i];
            if (item == null) continue;

            if (item.BlockType == blockType && item.CanRaiseCount())
            {
                item.RaiseCount();
                ItemsUpdated(this, new InventoryEventArgs());
                return;
            }
        }

        for (int i = 0; i < ItemCount; i++)
        {
            InventoryItem item = Items[i];
            if (item == null)
            {
                Items[i] = new InventoryItem(blockType);
                ItemsUpdated(this, new InventoryEventArgs());
                return;
            }           
        }

        return;
    }

    public InventoryItem TakeItem()
    {
        return TakeItem(CurrentlySelected);
    }

    public InventoryItem TakeItem(int index)
    {
        if (Items[index] == null) return null;

        InventoryItem item = Items[index];

        if (item.GetCount() > 1)
        {
            item.LowerCount();
        }
        else
        {
            Items[index] = null;
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
    }
}

public class InventoryEventArgs : EventArgs
{
    public InventoryEventArgs() {}
}