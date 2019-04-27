using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    BLOCK_AIR = 0,
    BLOCK_GRASS,
    BLOCK_DIRT,
    BLOCK_STONE,
    BLOCK_TORCH
}

public class BlockManager : MonoBehaviour
{
    public static BlockManager main;
    public Dictionary<BlockType, GameObject> pool = new Dictionary<BlockType, GameObject>();

    [Header("Blocks")]
    public GameObject[] BlockPrefabs;

    public BlockManager()
    {
        main = this;
    }

    public GameObject Add(BlockType blockType)
    {
        if (!pool.ContainsKey(blockType))
        {
            GameObject spawnedBlock = Instantiate(BlockPrefabs[(int)blockType - 1], Vector3.zero, Quaternion.identity);
            spawnedBlock.SetActive(false);

            pool.Add(blockType, spawnedBlock);

            return spawnedBlock;
        }

        return null;
    }

    public GameObject GetBlock(BlockType blockType)
    {
        if (pool.TryGetValue(blockType, out GameObject value))
        {
            return value;
        }
        else
        {
            return Add(blockType);
        }
    }
}
