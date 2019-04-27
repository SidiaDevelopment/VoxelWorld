using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    [Header("Chunk Generation")]
    public BlockManager BlockManagerInstance;
    public WorldRoot WorldRootInstance;
    public float PerlinAmplifier = 10f;
    public float PerlinFrequency = 20f;
    public float PerlinSeed = 200f;
    public bool NeedsUpdate = false;

    [Header("Transform")]
    public int PositionX;
    public int PositionZ;
    public int ChunkSize;
    public int ChunkHeight;
    public int MaxChunks;

    [Header("Internal")]
    public bool IsInitialised = false;
    public bool IsInitialising = false;

    // Update is called once per frame
    void Update()
    {
        if (!NeedsUpdate) return;
        NeedsUpdate = false;

        StartCoroutine(ChunkUpdate());
    }

    IEnumerator ChunkUpdate()
    {
        if (!IsInitialised && !IsInitialising)
        {
            IsInitialising = true;
            GenerateHeightMap();
        }

        ChunkMeshGenerator meshGenerator = new ChunkMeshGenerator();
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    UpdateBlock(x, y, z);
                    Vector3 position = new Vector3(x, y, z);
                    meshGenerator.AddBlock(GetChunkBorderX() + x, y, GetChunkBorderZ() + z, position);
                }
                yield return new WaitForEndOfFrame();

            }

        }

        meshGenerator.Combine(gameObject);

        yield break;
    }

    private void UpdateBlock(int x, int y, int z)
    {
        if (WorldRoot.BlockCache[GetChunkBorderX() + x, y, GetChunkBorderZ() + z].NeedsUpdate())
        {
            // Future interaction
            WorldRoot.BlockCache[GetChunkBorderX() + x, y, GetChunkBorderZ() + z].HasUpdated();
        }
    }

    private void GenerateHeightMap()
    {
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                int currentX = GetChunkBorderX() + x;
                int currentZ = GetChunkBorderZ() + z;

                int stoneMaxY = Mathf.FloorToInt(
                    Mathf.PerlinNoise(
                        (PerlinSeed * 2 + currentX) / PerlinFrequency,
                        (PerlinSeed + currentZ) / PerlinFrequency
                    ) * PerlinAmplifier
                ) + 5;

                int dirtMaxY = Mathf.FloorToInt(
                    Mathf.PerlinNoise(
                        (PerlinSeed + currentX) / PerlinFrequency,
                        (PerlinSeed + currentZ) / PerlinFrequency
                    ) * PerlinAmplifier
                ) + 10;

                for (int y = 0; y < ChunkHeight; y++)
                {
                    if (y < stoneMaxY)
                    {
                        WorldRoot.main.GetBlock(currentX, y, currentZ).NextBlock = BlockType.BLOCK_STONE;
                    }
                    else if (y < dirtMaxY)
                    {
                        WorldRoot.main.GetBlock(currentX, y, currentZ).NextBlock = BlockType.BLOCK_DIRT;
                    }
                    else if (y == dirtMaxY)
                    {
                        WorldRoot.main.GetBlock(currentX, y, currentZ).NextBlock = BlockType.BLOCK_GRASS;
                    }
                    else
                    {
                        WorldRoot.main.GetBlock(currentX, y, currentZ).NextBlock = BlockType.BLOCK_AIR;
                    }
                }
            }
        }
    }

    private int GetChunkBorderX()
    {
        return ChunkSize * PositionX;
    }

    private int GetChunkBorderZ()
    {
        return ChunkSize * PositionZ;
    }
}
