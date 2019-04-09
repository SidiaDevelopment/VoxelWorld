using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] public bool ShouldCombine = true;

    public virtual void UpdateFaces(int x, int y, int z, int chunkHeight, int chunkSize, BlockTypes[,,] voxelIndex)
    {
        List<BlockTypes> nonBlocking = new List<BlockTypes>()
        {
            BlockTypes.BLOCK_AIR,
            BlockTypes.BLOCK_TORCH,
        };

        transform.Find("Top").gameObject.SetActive(y >= chunkHeight - 1 || nonBlocking.IndexOf(voxelIndex[x, y + 1, z]) != -1);
        transform.Find("Bottom").gameObject.SetActive(y <= 0 || nonBlocking.IndexOf(voxelIndex[x, y - 1, z]) != -1);
        transform.Find("Back").gameObject.SetActive(x >= chunkSize - 1 || nonBlocking.IndexOf(voxelIndex[x + 1, y, z]) != -1);
        transform.Find("Front").gameObject.SetActive(x <= 0 || nonBlocking.IndexOf(voxelIndex[x - 1, y, z]) != -1);
        transform.Find("Left").gameObject.SetActive(z >= chunkSize - 1 || nonBlocking.IndexOf(voxelIndex[x, y, z + 1]) != -1);
        transform.Find("Right").gameObject.SetActive(z <= 0 || nonBlocking.IndexOf(voxelIndex[x, y, z - 1]) != -1);
    }
}
