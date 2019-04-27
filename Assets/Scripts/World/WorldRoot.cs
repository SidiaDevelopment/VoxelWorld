using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class WorldRoot : MonoBehaviour
{
    public static WorldRoot main;

    [Header("WorldGeneration")]
    public GameObject WorldChunkPrefab;
    public BlockManager BlockManagerInstance;

    [Header("World Size")]
    public int RenderDistance = 8;
    public int MaxChunks = 511;
    public int ChunkSize = 16;
    public int ChunkHeight = 256;

    [Header("Player Tracking")]
    public GameObject Player;
    [SerializeField] private Vector2Int CurrentPlayerChunk;
    [SerializeField] private Vector2Int LastPlayerChunk;

    [Header("Internal")]
    [SerializeField] private bool IsInitialised = false;
    [SerializeField] private bool IsInitialising = false;
    private GameObject[,] ChunkCache;
    public static ChunkBlock[,,] BlockCache;

    public ChunkBlock GetBlock(int x, int y, int z)
    {
        if (x >= MaxChunks * ChunkSize || x < 0)
        {
            return null;
        }

        if (y >= ChunkHeight || y < 0)
        {
            return null;
        }

        if (z >= MaxChunks * ChunkSize || z < 0)
        {
            return null;
        }

        if (BlockCache[x, y, z] == null)
        {
            BlockCache[x, y, z] = new ChunkBlock();
        }

        return BlockCache[x, y, z];
    }

    private void Start()
    {
        main = this;
    }

    private void Update()
    {
        if (!IsInitialised && !IsInitialising)
        {
            IsInitialising = true;

            ChunkCache = new GameObject[MaxChunks, MaxChunks];
            BlockCache = new ChunkBlock[MaxChunks * ChunkSize, ChunkHeight, MaxChunks * ChunkSize];
            CurrentPlayerChunk = new Vector2Int();
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

    IEnumerator ChunkGenerator()
    {
        for (int x = CurrentPlayerChunk.x - RenderDistance * 2; x <= CurrentPlayerChunk.x + RenderDistance * 2; x++)
        {
            for (int z = CurrentPlayerChunk.y - RenderDistance * 2; z <= CurrentPlayerChunk.y + RenderDistance * 2; z++)
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
            GameObject cachedChunk = ChunkCache[x, z];
            if (cachedChunk && !cachedChunk.activeSelf)
            {
                cachedChunk.SetActive(true);
            }
            else if (!cachedChunk)
            {
                GenerateChunk(x, z, IsPlayerChunk(x, z) && IsInitialising);
            }
        }
        else
        {
            GameObject cachedChunk;
            if (cachedChunk = ChunkCache[x, z])
            {
                cachedChunk.SetActive(false);
            }
        }
    }

    private void GenerateChunk(int x, int z, bool isSpawnChunk)
    {
        Vector3 spawnPosition = transform.position;

        spawnPosition.x -= MaxChunks * ((float)ChunkSize / 2f) - x * ChunkSize;
        spawnPosition.z -= MaxChunks * ((float)ChunkSize / 2f) - z * ChunkSize;

        GameObject spawnChunk = Instantiate(WorldChunkPrefab, spawnPosition, Quaternion.identity);
        spawnChunk.name = $"WorldChunk {x}_{z}";
        spawnChunk.transform.parent = transform;

        WorldChunk chunkScript = spawnChunk.GetComponent<WorldChunk>();
        chunkScript.BlockManagerInstance = BlockManagerInstance;
        chunkScript.WorldRootInstance = this;

        chunkScript.ChunkSize = ChunkSize;
        chunkScript.ChunkHeight = ChunkHeight;
        chunkScript.PositionX = x;
        chunkScript.PositionZ = z;
        chunkScript.MaxChunks = MaxChunks;

        chunkScript.NeedsUpdate = true;

        ChunkCache[x, z] = spawnChunk;
    }

    private bool IsInRenderDistance(int x, int z)
    {
        bool isInX = CurrentPlayerChunk.x - RenderDistance <= x && CurrentPlayerChunk.x + RenderDistance >= x;
        bool isInY = CurrentPlayerChunk.y - RenderDistance <= z && CurrentPlayerChunk.y + RenderDistance >= z;

        return (isInX && isInY);
    }

    private bool IsPlayerChunk(int x, int z)
    {
        return (x == CurrentPlayerChunk.x && z == CurrentPlayerChunk.y);
    }

    private Vector2Int GetCurrentPlayerChunk()
    {
        float playerX = Player.transform.position.x;
        float playerZ = Player.transform.position.z;

        playerX += ChunkSize * MaxChunks / 2f;
        playerZ += ChunkSize * MaxChunks / 2f;

        int x = Mathf.CeilToInt(playerX / ChunkSize);
        int z = Mathf.CeilToInt(playerZ / ChunkSize);

        // Chunk coordinates player is in
        return new Vector2Int(x - 1, z - 1);
    }

}
