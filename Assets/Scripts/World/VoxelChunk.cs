using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;

public class VoxelChunk : MonoBehaviour
{
    [SerializeField] public GameObject DefaultVoxel;
    [SerializeField] public bool NeedsUpdate = false;
    [SerializeField] public bool IsInitialised = false;

    [SerializeField] public int PositionX;
    [SerializeField] public int PositionZ;
    [SerializeField] public int ChunkSize = 16;
    [SerializeField] public int ChunkHeight = 256;

    [SerializeField] public float PerlinAmplifier = 10f;
    [SerializeField] public float PerlinFrequency = 20f;
    [SerializeField] public float PerlinSeed = 99;

    [NonSerialized] public GameObject[,,] VoxelInstances;
    [NonSerialized] public BlockTypes[,,] VoxelIndex;
    [NonSerialized] public BlockTypes[,,] CurrentVoxels;

    public void PlaceVoxel(int x, int y, int z, BlockTypes voxelType = BlockTypes.BLOCK_GRASS)
    {
        if (!IsInitialised) return;
        if (VoxelIndex[x, y, z] != BlockTypes.BLOCK_AIR) return;

        VoxelIndex[x, y, z] = voxelType;
        NeedsUpdate = true;
    }

    public void RemoveVoxel(int x, int y, int z)
    {
        if (!IsInitialised) return;

        VoxelIndex[x, y, z] = BlockTypes.BLOCK_AIR;
        NeedsUpdate = true;
    }

    public void UpdateVoxel(int x, int y, int z, BlockTypes voxelType)
    {
        if (!IsInitialised) return;

        VoxelIndex[x, y, z] = voxelType;
        NeedsUpdate = true;
    }
}

public enum BlockTypes : int
{
    BLOCK_AIR = 0,
    BLOCK_GRASS
}

public class VoxelChunkSystem : ComponentSystem
{
    private struct ChunkData
    {
        public VoxelChunk chunkObject;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var chunk in GetEntities<ChunkData>())
        {
            if (!chunk.chunkObject.NeedsUpdate) continue;
            chunk.chunkObject.NeedsUpdate = false;

            IEnumerator chunkUpdate = ChunkUpdate(chunk);
            MonoBehaviour routineProxy = chunk.chunkObject;
            routineProxy.StartCoroutine(chunkUpdate);
        }
    }

    IEnumerator ChunkUpdate(ChunkData chunkData)
    {
        VoxelChunk chunk = chunkData.chunkObject;

        if (!chunk.IsInitialised)
        {
            chunk.VoxelIndex = GenerateVoxelIndex(chunk, chunkData.transform);
            chunk.CurrentVoxels = new BlockTypes[chunk.ChunkSize, chunk.ChunkHeight, chunk.ChunkSize];
            chunk.VoxelInstances = new GameObject[chunk.ChunkSize, chunk.ChunkHeight, chunk.ChunkSize];

            yield return new WaitForEndOfFrame();
        }

        for (int x = 0; x < chunk.ChunkSize; x++)
        {
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    if (chunk.VoxelIndex[x, y, z] == BlockTypes.BLOCK_AIR)
                    {
                        if (chunk.VoxelInstances[x, y, z])
                        {
                            GameObject.Destroy(chunk.VoxelInstances[x, y, z]);
                            chunk.VoxelInstances[x, y, z] = null;
                        }
                    }
                    else if (chunk.CurrentVoxels[x, y, z] != chunk.VoxelIndex[x, y, z])
                    {
                        chunk.VoxelInstances[x, y, z] = CreateVoxel(x, y, z, chunk, chunkData.transform);
                        chunk.CurrentVoxels[x, y, z] = chunk.VoxelIndex[x, y, z];
                    }
                }

                if (!chunk.IsInitialised)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        MeshCombiner.combineMeshWithMaterials(chunk.gameObject);

        if (!chunk.IsInitialised)
        {
            chunk.IsInitialised = true;
        }

        yield break;
    }

    private BlockTypes[,,] GenerateVoxelIndex(VoxelChunk chunk, Transform transform)
    {
        BlockTypes[,,] voxelIndex = new BlockTypes[chunk.ChunkSize, chunk.ChunkHeight, chunk.ChunkSize];
        Vector3 position = transform.position;
        float positionX, positionZ;
        int maxY;

        for (int x = 0; x < chunk.ChunkSize; x++)
        {
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                positionX = position.x + x;
                positionZ = position.z + z;

                maxY = Mathf.FloorToInt(
                    Mathf.PerlinNoise(
                        (chunk.PerlinSeed + positionX) / chunk.PerlinFrequency,
                        (chunk.PerlinSeed + positionZ) / chunk.PerlinFrequency
                    ) * chunk.PerlinAmplifier
                );

                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    if (y <= maxY)
                    {
                        voxelIndex[x, y, z] = BlockTypes.BLOCK_GRASS;
                    }
                    else
                    {
                        voxelIndex[x, y, z] = BlockTypes.BLOCK_AIR;
                    }
                }
            }
        }
        

        return voxelIndex;
    }

    private GameObject CreateVoxel(int x, int y, int z, VoxelChunk chunk, Transform transform)
    {
        Vector3 position = transform.position;

        position.x += x + 0.5f;
        position.y += y + 0.5f;
        position.z += z + 0.5f;

        GameObject instance = GameObject.Instantiate(chunk.DefaultVoxel, position, Quaternion.identity);
        instance.transform.parent = transform;
        instance.SetActive(false);

        UpdateVoxel(x, y, z, chunk, ref instance);

        return instance;
    }

    public void UpdateVoxel(int x, int y, int z, VoxelChunk chunk, ref GameObject voxel)
    {
        voxel.transform.Find("Top").gameObject.SetActive(y >= chunk.ChunkHeight - 1 || chunk.VoxelIndex[x, y + 1, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Bottom").gameObject.SetActive(y <= 0 || chunk.VoxelIndex[x, y - 1, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Back").gameObject.SetActive(x >= chunk.ChunkSize - 1 || chunk.VoxelIndex[x + 1, y, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Front").gameObject.SetActive(x <= 0 || chunk.VoxelIndex[x - 1, y, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Left").gameObject.SetActive(z >= chunk.ChunkSize - 1 || chunk.VoxelIndex[x, y, z + 1] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Right").gameObject.SetActive(z <= 0 || chunk.VoxelIndex[x, y, z - 1] == BlockTypes.BLOCK_AIR);
    }
}