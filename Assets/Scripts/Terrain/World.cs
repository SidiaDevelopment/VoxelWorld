using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;

// The main world generating all chunks
public class World : MonoBehaviour
{
    // Chunks that are generated
    public GameObject chunk;

    // How far can a player see
    public int renderDistance = 5;

    // How far do we generate chunks
    public int maxChunks = 3;

    // How big is a chunk
    public int chunkSize = 16;

    // Holding all generated chunks
    [NonSerialized] public GameObject[,] chunks;

    // Is the world initialised
    [NonSerialized] public bool initialised = false;
}

// Manages our world, potential all worlds
public class WorldSystem : ComponentSystem
{
    // World Archetype
    struct WorldArchetype
    {
        public World world;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var w in GetEntities<WorldArchetype>())
        {
            if (!w.world.initialised)
            {
                w.world.chunks = new GameObject[w.world.maxChunks, w.world.maxChunks];
                w.world.initialised = true;
            }

            // Get current chunk
            Vector2Int playerChunk = GetActiveChunk(w.world.maxChunks, w.world.chunkSize);

            // Generate all relevant chunks
            IEnumerator coroutine = ChunkGenerator(w, playerChunk);
            w.world.StartCoroutine(coroutine);
        }
    }

    // Generates all relevant chunks
    IEnumerator ChunkGenerator(WorldArchetype w, Vector2Int playerChunk)
    {
        for (int x = 0; x < w.world.maxChunks; x++)
        {
            for (int y = 0; y < w.world.maxChunks; y++)
            {
                // Is current chunk coordinate in renderDistance of player
                bool isInX = playerChunk.x - w.world.renderDistance < x && playerChunk.x + w.world.renderDistance > x;
                bool isInY = playerChunk.y - w.world.renderDistance < y && playerChunk.y + w.world.renderDistance > y;

                // Is the player currently in this chunk
                bool isPlayerChunk = (playerChunk.x == x && playerChunk.y == y);

                if (!isInX || !isInY)
                {
                    // Chunk out of range
                    if (w.world.chunks[x, y])
                    {
                        w.world.chunks[x, y].SetActive(false);
                    }

                    continue;
                }

                if (!w.world.chunks[x, y])
                {
                    // Chunk in range but never generated
                    w.world.chunks[x, y] = GenerateChunk(x, y, w.world, w.transform, isPlayerChunk);
                }
                else
                {
                    // Chunk in range but cached
                    if (!w.world.chunks[x, y].activeSelf)
                    {
                        w.world.chunks[x, y].SetActive(true);
                    }
                }
            }
        }

        yield break;
    }

    // Generates a brand new chunk that gets picked up by the ChunkSystem
    private GameObject GenerateChunk(int x, int y, World world, Transform transform, bool isPlayerChunk = false)
    {
        Vector3 position = transform.position;

        // Move chunks base position so [0, 0] is actually the center of the world and not the origin
        position.x += -((float)world.maxChunks * (float)world.chunkSize / 2);
        position.z += -((float)world.maxChunks * (float)world.chunkSize / 2);

        position.x += x * world.chunkSize;
        position.z += y * world.chunkSize;

        GameObject newChunk = GameObject.Instantiate(world.chunk, position, Quaternion.identity);
        newChunk.gameObject.GetComponent<Chunk>().chunkSize = world.chunkSize;
        newChunk.gameObject.GetComponent<Chunk>().needsUpdate = true;
        newChunk.gameObject.GetComponent<Chunk>().isPlayerChunk = isPlayerChunk;

        newChunk.transform.parent = world.transform;

        return newChunk;
    }

    // Get current chunk that player is in
    private Vector2Int GetActiveChunk(int maxChunks, int chunkSize)
    {
        // Find current player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float playerX = player.transform.position.x;
        float playerY = player.transform.position.z;

        // Move chunks base position so [0, 0] is actually the center of the world and not the origin
        playerX += chunkSize * maxChunks / 2f;
        playerY += chunkSize * maxChunks / 2f;

        int x = Mathf.CeilToInt(playerX / chunkSize);
        int y = Mathf.CeilToInt(playerY / chunkSize);

        // Chunk coordinates player is in
        return new Vector2Int(x - 1, y - 1);
    }
}
