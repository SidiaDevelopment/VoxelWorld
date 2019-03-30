using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;

// A chunk in the world that manages terrain
public class VolumeChunk : MonoBehaviour
{
    // GameObject used to represent a 1x1 voxel in the world
    public GameObject voxel;

    // Contains if this chunk needs an update
    public bool needsUpdate = true;

    // Chunk size that is set by the WorldSystem's chunk size
    [NonSerialized] public int chunkSize = 16;

    // An array of all generated voxels
    [NonSerialized] public GameObject[,,] voxels;

    // Is set to true if player is currently in this chunk
    [NonSerialized] public bool isPlayerChunk = false;

    // Manages if this chunk has already been initialised
    [NonSerialized] public bool initialised = false;

    [NonSerialized] public int[,,] voxelTypes;

    public void RemoveVoxel(int x, int y, int z)
    {
        Destroy(voxels[x - 1, y - 1, z - 1]);
        voxelTypes[x - 1, y - 1, z - 1] = 0;
        UpdateVoxelSurroundingVoxels(x - 1, y - 1, z - 1);
        needsUpdate = true;
    }

    public void CreateVoxel(int x, int y, int z)
    {
        voxelTypes[x - 1, y - 1, z - 1] = 1;
        GenerateVoxel(x - 1, y - 1, z - 1);
        UpdateVoxelSurroundingVoxels(x - 1, y - 1, z - 1);
        needsUpdate = true;
    }

    public void UpdateVoxelSurroundingVoxels(int x, int y, int z)
    {
        if (x < chunkSize - 1)
        {
            if (voxels[x + 1, y, z])
                UpdateVoxel(x + 1, y, z, ref voxels[x + 1, y, z]);
        }

        if (x > 0)
        {
            if (voxels[x - 1, y, z])
                UpdateVoxel(x - 1, y, z, ref voxels[x - 1, y, z]);
        }

        if (y < 254)
        {
            if (voxels[x, y + 1, z])
                UpdateVoxel(x, y + 1, z, ref voxels[x, y + 1, z]);
        }

        if (y > 0)
        {
            if (voxels[x, y - 1, z])
                UpdateVoxel(x, y - 1, z, ref voxels[x, y - 1, z]);
        }

        if (z < chunkSize - 1)
        {
            if (voxels[x, y, z + 1])
                UpdateVoxel(x, y, z + 1, ref voxels[x, y, z + 1]);
        }

        if (z > 0)
        {
            if (voxels[x, y, z - 1])
                UpdateVoxel(x, y, z - 1, ref voxels[x, y, z - 1]);
        }
    }

    public void UpdateVoxel(int x, int y, int z, ref GameObject voxel)
    {
        voxel.transform.Find("Top").gameObject.SetActive(voxelTypes[x, y + 1, z] != 1);
        voxel.transform.Find("Bottom").gameObject.SetActive(!(y > 0 && voxelTypes[x, y - 1, z] == 1));
        voxel.transform.Find("Front").gameObject.SetActive(!(x < chunkSize - 1 && voxelTypes[x + 1, y, z] == 1));
        voxel.transform.Find("Back").gameObject.SetActive(!(x > 0 && voxelTypes[x - 1, y, z] == 1));
        voxel.transform.Find("Right").gameObject.SetActive(!(z < chunkSize - 1 && voxelTypes[x, y, z + 1] == 1));
        voxel.transform.Find("Left").gameObject.SetActive(!(z > 0 && voxelTypes[x, y, z - 1] == 1));
    }

    // Generate a new voxel and fill the height gap with more voxels
    public void GenerateVoxel(int x, int y, int z)
    {
        if (voxelTypes[x, y, z] == 1)
        {
            // Take chunk position as reference
            Vector3 position = transform.position;

            position.x += x + 0.5f;
            position.z += z + 0.5f;
            position.y += +y + 0.5f;
            voxel.gameObject.SetActive(false);
            GameObject newVoxel = GameObject.Instantiate(voxel, position, Quaternion.identity);
            newVoxel.transform.parent = transform;

            UpdateVoxel(x, y, z, ref newVoxel);

            if (voxels[x, y, z])
            {
                GameObject.Destroy(voxels[x, y, z]);
            }

            voxels[x, y, z] = newVoxel.gameObject;
        }
    }
}

// This System manages all chunks
// Currently generates terrain for each chunk with a perlin noise as height, gaps in height get filled
// Current TODO:
//  - Test to yield after multiple voxels to speed up generation without creating lag
//  - Mix two perlin noises without creating considerable lag for better and more random terrain
public class VolumeChunkSystem : ComponentSystem
{
    // Amplification of perlin noise
    private float amp = 10f;

    // Frequency of perlin noise
    private float frq = 20f;

    // World seed
    private float seed = 99;

    // Chunk Archetype
    public struct ChunkArchetype
    {
        public VolumeChunk chunk;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var c in GetEntities<ChunkArchetype>())
        {
            // Update chunk if needs update
            if (!c.chunk.needsUpdate) continue;
            c.chunk.needsUpdate = false;

            IEnumerator terrainGenerationCoroutine = TerrainGenerator(c);
            c.chunk.StartCoroutine(terrainGenerationCoroutine);
        }
    }

    // Generate terrain for Chunk c.chunk
    IEnumerator TerrainGenerator(ChunkArchetype c)
    {
        // If already initialised just generate the final mesh again
        if (!c.chunk.initialised)
        {
            // Initialize voxels array
            c.chunk.voxels = new GameObject[c.chunk.chunkSize, 256, c.chunk.chunkSize];

            // Get perlin noise heights
            c.chunk.voxelTypes = GenerateTerrainHeight(c);

            for (int x = 0; x < c.chunk.chunkSize; x++)
            {
                for (int z = 0; z < c.chunk.chunkSize; z++)
                {
                    for (int y = 0; y < 256; y++)
                    {
                        // Generate a new voxel at position [x,y, z]
                        c.chunk.GenerateVoxel(x, y, z);
                        
                    }
                    yield return new WaitForEndOfFrame();

                }
            }

            c.chunk.initialised = true;
        }

        MeshCombiner.combineMeshWithMaterials(c.chunk.gameObject);

        yield break;
    }

    // Pre-generate perlin noise
    // Generates with 1 unit overhead to each side to account for the height gap fill algorithm on the border of chunks
    private int[,,] GenerateTerrainHeight(ChunkArchetype c)
    {
        int[,,] voxelTypes = new int[c.chunk.chunkSize, 256, c.chunk.chunkSize];

        for (int x = 0; x < c.chunk.chunkSize; x++)
        {
            for (int z = 0; z < c.chunk.chunkSize; z++)
            {
                Vector3 position = c.transform.position;

                // Add current voxel position, the subtraction accounts for the overhead we generate
                position.x += x;
                position.z += z;

                int baseY = Mathf.FloorToInt(Mathf.PerlinNoise((1000000f + position.x) / frq, (seed + 1000000f + position.z) / frq) * amp + 10);
                for (int y = 0; y < baseY; y++)
                {
                    // Generate perlin noise height
                    voxelTypes[x, y, z] = 1;
                }
            }
        }

        return voxelTypes;
    }
}
