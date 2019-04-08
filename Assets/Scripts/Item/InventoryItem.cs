using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public BlockTypes BlockType;
    private int Count;

    public InventoryItem(BlockTypes blockType, int count = 1)
    {
        BlockType = blockType;
        Count = count;
    }

    public int GetCount()
    {
        return Count;
    }

    public void SetCount(int count)
    {
        if (count > 0 && count <= 64)
        {
            Count = count;
        }
    }

    public void RaiseCount()
    {
        if (CanRaiseCount())
        {
            Count++;
        }
    }

    public void LowerCount()
    {
        if (CanLowerCount())
        {
            Count--;
        }
    }

    public bool CanLowerCount()
    {
        return Count > 1;
    }

    public bool CanRaiseCount()
    {
        return Count < 64;
    }
}
