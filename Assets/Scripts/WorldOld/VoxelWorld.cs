using System;
using System.Collections;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    [SerializeField] public GameObject VoxelChunkPrefab;
    [SerializeField] public int RenderDistance = 5;
    [SerializeField] public int MaxChunks = 255;
    [SerializeField] public int ChunkSize = 16;
    [SerializeField] public int ChunkHeight = 256;

    [SerializeField] public bool IsInitialised = false;
    [SerializeField] public bool IsInitialising = false;

    [SerializeField] public Vector2Int CurrentPlayerChunk;
    [SerializeField] public Vector2Int LastPlayerChunk;

    [NonSerialized] public GameObject[,] ChunkCache;

    void Update()
    {
        if (!IsInitialised && !IsInitialising)
        {
            ChunkCache = new GameObject[MaxChunks, MaxChunks];
            IsInitialising = true;
            LastPlayerChunk = new Vector2Int();
        }

        CurrentPlayerChunk = GetCurrentPlayerChunk();
        if (LastPlayerChunk == CurrentPlayerChunk)
        {
            return;
        }

        LastPlayerChunk = CurrentPlayerChunk;

        IEnumerator coroutine = ChunkGenerator();
        StartCoroutine(coroutine);
    }

    private Vector2Int GetCurrentPlayerChunk()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float playerX = player.transform.position.x;
        float playerZ = player.transform.position.z;

        playerX += ChunkSize * MaxChunks / 2f;
        playerZ += ChunkSize * MaxChunks / 2f;

        int x = Mathf.CeilToInt(playerX / ChunkSize);
        int z = Mathf.CeilToInt(playerZ / ChunkSize);

        // Chunk coordinates player is in
        return new Vector2Int(x - 1, z - 1);
    }

    IEnumerator ChunkGenerator()
    {
        for (int x = 0; x < MaxChunks; x++)
        {
            for (int z = 0; z < MaxChunks; z++)
            {
                UpdateChunk(x, z);
            }
        }

        if (!IsInitialised)
        {
            IsInitialising = false;
            IsInitialised = true;
        }

        yield break;
    }

    private void UpdateChunk(int x, int z)
    {
        if (IsInRenderDistance(x, z))
        {
            GameObject cachedChunk;
            if (cachedChunk = GetCachedChunk(x, z))
            {
                cachedChunk.SetActive(true);
            }
            else
            {
                GenerateChunk(x, z, IsPlayerChunk(x, z) && IsInitialising);
            }
        }
        else
        {
            GameObject cachedChunk;
            if (cachedChunk = GetCachedChunk(x, z))
            {
                cachedChunk.SetActive(false);
            }
        }
    }

    private bool IsInRenderDistance(int x, int z)
    {
        bool isInX = CurrentPlayerChunk.x - RenderDistance < x && CurrentPlayerChunk.x + RenderDistance > x;
        bool isInY = CurrentPlayerChunk.y - RenderDistance < z && CurrentPlayerChunk.y + RenderDistance > z;

        return (isInX && isInY);
    }

    private bool IsPlayerChunk(int x, int z)
    {
        return (x == CurrentPlayerChunk.x && z == CurrentPlayerChunk.y);
    }

    public GameObject GetCachedChunk(int x, int z)
    {
        if (x >= MaxChunks || z >= MaxChunks)
        {
            return null;
        }

        return ChunkCache[x, z];
    }

    private void GenerateChunk(int x, int z, bool isSpawnChunk)
    {
        Vector3 position = transform.position;

        position.x += -((float)MaxChunks * (float)ChunkSize / 2);
        position.z += -((float)MaxChunks * (float)ChunkSize / 2);

        position.x += x * ChunkSize;
        position.z += z * ChunkSize;

        GameObject newChunk = GameObject.Instantiate(VoxelChunkPrefab, position, Quaternion.identity);
        newChunk.gameObject.GetComponent<VoxelChunk>().ChunkSize = ChunkSize;
        newChunk.gameObject.GetComponent<VoxelChunk>().NeedsUpdate = true;
        newChunk.gameObject.GetComponent<VoxelChunk>().VoxelWorldInstance = this;
        newChunk.gameObject.GetComponent<VoxelChunk>().ChunkHeight = 256;
        newChunk.gameObject.GetComponent<VoxelChunk>().PositionX = x;
        newChunk.gameObject.GetComponent<VoxelChunk>().PositionZ = z;
        newChunk.gameObject.GetComponent<VoxelChunk>().SpawnPlayerAfterUpdate = isSpawnChunk;
        newChunk.name = $"Chunk {x}_{z}";

        newChunk.transform.parent = transform;

        ChunkCache[x, z] = newChunk;
    }
}
