using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backup : MonoBehaviour
{

    public GameObject BlockPrefab;
    public GameObject BlockInstance;
    public bool NeedsUpdate;
    public int ChunkSize = 16;
    public int ChunkHeight = 64;

    [SerializeField] public float PerlinAmplifier = 10f;
    [SerializeField] public float PerlinFrequency = 20f;
    [SerializeField] public float PerlinSeed = 99;
    // Start is called before the first frame update
    void Start()
    {
        BlockInstance = Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity);
        BlockInstance.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!NeedsUpdate) return;
        NeedsUpdate = false;

        StartCoroutine(BuildChunk());
    }

    IEnumerator BuildChunk()
    {
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    int border = Mathf.FloorToInt(
                        Mathf.PerlinNoise(
                            (PerlinSeed + x) / PerlinFrequency,
                            (PerlinSeed + z) / PerlinFrequency
                        ) * PerlinAmplifier
                    );

                    if (y > 32 + border) continue;

                    BlockInstance.transform.position = new Vector3(x - (ChunkSize / 2), y - (ChunkHeight / 2), z - (ChunkSize / 2));

                    foreach (Transform child in BlockInstance.transform)
                    {
                        bool shouldCombine = false;

                        switch (child.name)
                        {
                            case "Top":
                                if (y == 32 + border)
                                {
                                    shouldCombine = true;
                                }
                                break;
                            case "Bottom":
                                if (y == 0)
                                {
                                    shouldCombine = true;
                                }
                                break;
                            case "Right":
                                if (z == 0)
                                {
                                    shouldCombine = true;
                                }
                                break;
                            case "Left":
                                if (z == ChunkSize - 1)
                                {
                                    shouldCombine = true;
                                }
                                break;
                            case "Front":
                                if (x == 0)
                                {
                                    shouldCombine = true;
                                }
                                break;
                            case "Back":
                                if (x == ChunkSize - 1)
                                {
                                    shouldCombine = true;
                                }
                                break;
                        }

                        if (!shouldCombine)
                        {
                            continue;
                        }

                        MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                        MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

                        int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterial.name);
                        if (materialArrayIndex == -1)
                        {
                            materials.Add(meshRenderer.sharedMaterial);
                            materialArrayIndex = materials.Count - 1;
                        }
                        combineInstanceArrays.Add(new ArrayList());

                        CombineInstance combineInstance = new CombineInstance();

                        Vector3 getQuadLocalPositionFromParent = transform.InverseTransformPoint(meshFilter.transform.position);
                        combineInstance.transform = Matrix4x4.TRS(getQuadLocalPositionFromParent, meshFilter.transform.rotation, new Vector3(1, 1, 1));

                        combineInstance.subMeshIndex = 0;
                        combineInstance.mesh = meshFilter.sharedMesh;
                        (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        

        yield break;
    }

    private static int Contains(ArrayList searchList, string searchName)
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
