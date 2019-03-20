using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;

public class Chunk : MonoBehaviour
{
    public GameObject voxel;
    public int chunkSize = 16;

    public GameObject[,,] voxels;

    public bool needsUpdate = true;
    public bool isPlayerChunk = false;

    public bool instantiated = false;
}

public class ChunkSystem : ComponentSystem
{
    public float amp = 10f;
    public float frq = 20f;
    public float seed = 99;

    private int currentRow = 0;

    public struct Chunks
    {
        public Chunk chunk;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var c in GetEntities<Chunks>())
        {
            if (!c.chunk.needsUpdate) continue;
            c.chunk.needsUpdate = false;

            IEnumerator coroutine = TerrainGenerator(c);
            c.chunk.StartCoroutine(coroutine);
        }
    }

    IEnumerator TerrainGenerator(Chunks c)
    {
        if (!c.chunk.instantiated)
        {
            c.chunk.voxels = new GameObject[c.chunk.chunkSize, c.chunk.chunkSize, 256];
            int[,] perlinHeight = new int[c.chunk.chunkSize + 2, c.chunk.chunkSize + 2];

            for (int x = 0; x < c.chunk.chunkSize + 2; x++)
            {
                for (int y = 0; y < c.chunk.chunkSize + 2; y++)
                {
                    Vector3 position = c.transform.position;

                    position.x += x - 1;
                    position.z += y - 1;

                    perlinHeight[x, y] = Mathf.FloorToInt(Mathf.PerlinNoise((1000000f + position.x) / frq,
                    (seed + 1000000f + position.z) / frq) * amp + 10);
                }
            }

            for (int x = 0; x < c.chunk.chunkSize; x++)
            {
                for (int z = 0; z < c.chunk.chunkSize; z++)
                {
                    Vector3 position = c.transform.position;

                    position.x += x;
                    position.z += z;

                    float oldPositionY = position.y;

                    int perlinX = x + 1;
                    int perlinZ = z + 1;

                    int y = perlinHeight[perlinX, perlinZ];

                    int highestY = Mathf.Max(
                        y,
                        perlinHeight[perlinX + 1, perlinZ],
                        perlinHeight[perlinX - 1, perlinZ],
                        perlinHeight[perlinX, perlinZ + 1],
                        perlinHeight[perlinX, perlinZ - 1]
                    );

                    for (int fillY = y; fillY <= highestY; fillY++)
                    {
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

                    yield return new WaitForEndOfFrame();
                }
            }

            c.chunk.instantiated = true;
        }

        combineMesh(c.chunk.gameObject, c.chunk.chunkSize);

        

        yield break;
    }

    private void combineMesh(GameObject chunk, int chunkSize)
    {
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer || !meshFilter.sharedMesh || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                continue;
            }

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

        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        meshFilterCombine.sharedMesh.RecalculateBounds();
        meshFilterCombine.sharedMesh.RecalculateNormals();
        meshFilterCombine.gameObject.AddComponent<MeshCollider>();

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
