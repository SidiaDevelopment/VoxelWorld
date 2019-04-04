using System;
using System.Collections;
using UnityEngine;

public enum BlockTypes : int
{
    BLOCK_AIR = 0,
    BLOCK_GRASS
}

public class VoxelChunk : MonoBehaviour
{
    [SerializeField] public GameObject DefaultVoxel;
    [SerializeField] public bool NeedsUpdate = false;
    [SerializeField] public bool IsInitialised = false;
    [SerializeField] public bool IsInitialising = false;

    [SerializeField] public int PositionX;
    [SerializeField] public int PositionZ;
    [SerializeField] public int ChunkSize = 16;
    [SerializeField] public int ChunkHeight = 256;
    [SerializeField] public bool SpawnPlayerAfterUpdate;

    [SerializeField] public float PerlinAmplifier = 10f;
    [SerializeField] public float PerlinFrequency = 20f;
    [SerializeField] public float PerlinSeed = 99;

    [NonSerialized] public VoxelWorld VoxelWorldInstance;
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

    public void UpdateVoxelFaces(int x, int y, int z)
    {
        GameObject voxel = VoxelInstances[x, y, z];

        voxel.transform.Find("Top").gameObject.SetActive(y >= ChunkHeight - 1 || VoxelIndex[x, y + 1, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Bottom").gameObject.SetActive(y <= 0 || VoxelIndex[x, y - 1, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Back").gameObject.SetActive(x >= ChunkSize - 1 || VoxelIndex[x + 1, y, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Front").gameObject.SetActive(x <= 0 || VoxelIndex[x - 1, y, z] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Left").gameObject.SetActive(z >= ChunkSize - 1 || VoxelIndex[x, y, z + 1] == BlockTypes.BLOCK_AIR);
        voxel.transform.Find("Right").gameObject.SetActive(z <= 0 || VoxelIndex[x, y, z - 1] == BlockTypes.BLOCK_AIR);
    }

    public void UpdateSurroundingVoxelFaces(int x, int y, int z)
    {
        if (x < ChunkSize - 1)
        {
            if (VoxelInstances[x + 1, y, z])
                UpdateVoxelFaces(x + 1, y, z);
        }

        if (x > 0)
        {
            if (VoxelInstances[x - 1, y, z])
                UpdateVoxelFaces(x - 1, y, z);
        }

        if (y < ChunkHeight - 1)
        {
            if (VoxelInstances[x, y + 1, z])
                UpdateVoxelFaces(x, y + 1, z);
        }

        if (y > 0)
        {
            if (VoxelInstances[x, y - 1, z])
                UpdateVoxelFaces(x, y - 1, z);
        }

        if (z < ChunkSize - 1)
        {
            if (VoxelInstances[x, y, z + 1])
                UpdateVoxelFaces(x, y, z + 1);
        }

        if (z > 0)
        {
            if (VoxelInstances[x, y, z - 1])
                UpdateVoxelFaces(x, y, z - 1);
        }
    }

    void Update()
    {

        if (!NeedsUpdate) return;
        NeedsUpdate = false;

        IEnumerator chunkUpdate = ChunkUpdate();
        StartCoroutine(chunkUpdate);
    }

    IEnumerator ChunkUpdate()
    {
        if (!IsInitialised)
        {
            VoxelIndex = GenerateVoxelIndex();
            CurrentVoxels = new BlockTypes[ChunkSize, ChunkHeight, ChunkSize];
            VoxelInstances = new GameObject[ChunkSize, ChunkHeight, ChunkSize];
            IsInitialising = true;

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("updating " + gameObject.name);

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    UpdateVoxel(x, y, z);
                }

                if (IsInitialising)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        MeshCombiner.combineMeshWithMaterials(VoxelInstances, gameObject);

        if (SpawnPlayerAfterUpdate)
        {
            SpawnPlayerAfterUpdate = false;
            SpawnPlayer();
        }

        if (IsInitialising && !IsInitialised)
        {
            IsInitialised = true;
            IsInitialising = false;
        }

        yield break;
    }

    private BlockTypes[,,] GenerateVoxelIndex()
    {
        BlockTypes[,,] voxelIndex = new BlockTypes[ChunkSize, ChunkHeight, ChunkSize];
        Vector3 position = transform.position;
        float positionX, positionZ;
        int maxY;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int z = 0; z < ChunkSize; z++)
            {
                positionX = position.x + x;
                positionZ = position.z + z;

                maxY = Mathf.FloorToInt(
                    Mathf.PerlinNoise(
                        (PerlinSeed + positionX) / PerlinFrequency,
                        (PerlinSeed + positionZ) / PerlinFrequency
                    ) * PerlinAmplifier
                );

                for (int y = 0; y < ChunkHeight; y++)
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

    private void UpdateVoxel(int x, int y, int z)
    {
        if (CurrentVoxels[x, y, z] != VoxelIndex[x, y, z])
        {
            if (VoxelInstances[x, y, z])
            {
                GameObject.Destroy(VoxelInstances[x, y, z]);
            }

            if (VoxelIndex[x, y, z] == BlockTypes.BLOCK_AIR)
            {
                VoxelInstances[x, y, z] = null;
                CurrentVoxels[x, y, z] = BlockTypes.BLOCK_AIR;
            }
            else
            {
                VoxelInstances[x, y, z] = CreateVoxel(x, y, z);
                CurrentVoxels[x, y, z] = VoxelIndex[x, y, z];
                UpdateVoxelFaces(x, y, z);
            }

            if (!IsInitialising)
            {
                UpdateSurroundingVoxelFaces(x, y, z);
            }
        }
    }

    private GameObject CreateVoxel(int x, int y, int z)
    {
        Vector3 position = transform.position;

        position.x += x + 0.5f;
        position.y += y + 0.5f;
        position.z += z + 0.5f;

        GameObject instance = GameObject.Instantiate(DefaultVoxel, position, Quaternion.identity);
        instance.name = $"Voxel {x}_{y}_{z}";
        instance.transform.parent = transform;
        instance.SetActive(false);

        return instance;
    }

    private void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector3 chunkPosition = transform.position;
        Vector3 overChunkCenter = new Vector3(chunkPosition.x + (float)ChunkSize / 2, chunkPosition.y + ChunkHeight, chunkPosition.z + (float)ChunkSize / 2);

        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        RaycastHit hit;
        bool didRayCastHit = Physics.Raycast(overChunkCenter, player.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask);

        if (didRayCastHit)
        {
            Debug.Log("Found floor to spawn on: " + hit.collider.name);
            player.transform.position = new Vector3(chunkPosition.x + (float)ChunkSize / 2, hit.point.y + 5, chunkPosition.z + (float)ChunkSize / 2);
        }
        else
        {
            Debug.Log("Couldn't find floor to spawn on");
            player.transform.position = overChunkCenter;
        }

        player.GetComponent<Rigidbody>().useGravity = true;

    }
}