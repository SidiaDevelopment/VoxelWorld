using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;

// A chunk in the world that manages terrain
public class Chunk : MonoBehaviour
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
}

// This System manages all chunks
// Currently generates terrain for each chunk with a perlin noise as height, gaps in height get filled
// Current TODO:
//  - Test to yield after multiple voxels to speed up generation without creating lag
//  - Mix two perlin noises without creating considerable lag for better and more random terrain
public class ChunkSystem : ComponentSystem
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
        public Chunk chunk;
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
            c.chunk.voxels = new GameObject[c.chunk.chunkSize, c.chunk.chunkSize, 256];

            // Get perlin noise heights
            int[,] perlinHeight = GeneratePerlinNoise(c);

            for (int x = 0; x < c.chunk.chunkSize; x++)
            {
                for (int z = 0; z < c.chunk.chunkSize; z++)
                {
                    // Generate a new voxel at position [x,z]
                    GenerateVoxel(ref c, x, z, perlinHeight);

                    yield return new WaitForEndOfFrame();
                }
            }

            c.chunk.initialised = true;
        }

        // Combine everything to 
        combineMesh(c.chunk.gameObject, c.chunk.chunkSize);

        yield break;
    }

    // Pre-generate perlin noise
    // Generates with 1 unit overhead to each side to account for the height gap fill algorithm on the border of chunks
    private int[,] GeneratePerlinNoise(ChunkArchetype c)
    {
        int[,] perlinHeight = new int[c.chunk.chunkSize + 2, c.chunk.chunkSize + 2];

        for (int x = 0; x < c.chunk.chunkSize + 2; x++)
        {
            for (int y = 0; y < c.chunk.chunkSize + 2; y++)
            {
                // Take chunk position as reference
                Vector3 position = c.transform.position;

                // Add current voxel position, the subtraction accounts for the overhead we generate
                position.x += x - 1;
                position.z += y - 1;

                // Generate perlin noise height
                perlinHeight[x, y] = Mathf.FloorToInt(Mathf.PerlinNoise((1000000f + position.x) / frq, (seed + 1000000f + position.z) / frq) * amp + 10);
            }
        }

        return perlinHeight;
    }

    // Generate a new voxel and fill the height gap with more voxels
    private void GenerateVoxel(ref ChunkArchetype c, int x, int z, int[,] perlinHeight)
    {
        // Take chunk position as reference
        Vector3 position = c.transform.position;

        position.x += x;
        position.z += z;

        // Save our old position
        float oldPositionY = position.y;

        // Calculate against the overhead
        int perlinX = x + 1;
        int perlinZ = z + 1;

        // Get how many blocks high we have to fill
        int y = perlinHeight[perlinX, perlinZ];
        int highestY = Mathf.Max(
            y,
            perlinHeight[perlinX + 1, perlinZ],
            perlinHeight[perlinX - 1, perlinZ],
            perlinHeight[perlinX, perlinZ + 1],
            perlinHeight[perlinX, perlinZ - 1]
        );

        // Generate one voxel per height fill segment
        for (int fillY = y; fillY <= highestY; fillY++)
        {
            // Reuse position for all height voxels
            position.y = oldPositionY + fillY;
            GameObject newVoxel = GameObject.Instantiate(c.chunk.voxel, position, Quaternion.identity);
            newVoxel.transform.parent = c.transform;
            newVoxel.SetActive(false);

            if (c.chunk.voxels[x, z, fillY])
            {
                GameObject.Destroy(c.chunk.voxels[x, z, 0]);
            }

            c.chunk.voxels[x, z, fillY] = newVoxel.gameObject;
        }
    }

    // Combines all voxel of this chunk to one mesh while maintaining materials
    private void combineMesh(GameObject chunk, int chunkSize)
    {
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        // Get all voxel sides
        MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer || !meshFilter.sharedMesh || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                continue;
            }

            // Add meshes to list
            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                if (materialArrayIndex == -1)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count - 1;
                }
                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance();

                Vector3 getQuadLocalPositionFromChunk = chunk.transform.InverseTransformPoint(meshFilter.transform.position);
                combineInstance.transform = Matrix4x4.TRS(getQuadLocalPositionFromChunk, meshFilter.transform.rotation, new Vector3(1, 1, 1));

                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // Create objects for the combined mesh
        MeshFilter meshFilterCombine = chunk.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = chunk.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRendererCombine = chunk.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = chunk.AddComponent<MeshRenderer>();
        }

        // Combine meshes by material
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);
            meshes[m].RecalculateBounds();
            meshes[m].RecalculateNormals();

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine material meshes to one mesh
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        meshFilterCombine.sharedMesh.RecalculateBounds();
        meshFilterCombine.sharedMesh.RecalculateNormals();
        meshFilterCombine.gameObject.AddComponent<MeshCollider>();

        // Cleanup
        foreach (Mesh oldMesh in meshes)
        {
            oldMesh.Clear();
            GameObject.DestroyImmediate(oldMesh);
        }

        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        foreach (MeshFilter meshFilter in meshFilters)
        {
            GameObject.DestroyImmediate(meshFilter);
        }
    }

    // Helper function for combineMesh
    private int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }

        return -1;
    }
}
