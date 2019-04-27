using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBlock
{
    public BlockType CurrentBlock;
    public BlockType NextBlock;

    public bool NeedsUpdate()
    {
        return CurrentBlock != NextBlock;
    }

    public void HasUpdated()
    {
        CurrentBlock = NextBlock;
    }
}
