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
            for (int z = 0; z < w.world.maxChunks; z++)
            {
                // Is current chunk coordinate in renderDistance of player
                bool isInX = playerChunk.x - w.world.renderDistance < x && playerChunk.x + w.world.renderDistance > x;
                bool isInY = playerChunk.y - w.world.renderDistance < z && playerChunk.y + w.world.renderDistance > z;

                // Is the player currently in this chunk
                bool isPlayerChunk = (playerChunk.x == x && playerChunk.y == z);

                if (!isInX || !isInY)
                {
                    // Chunk out of range
                    if (w.world.chunks[x, z])
                    {
                        w.world.chunks[x, z].SetActive(false);
                    }

                    continue;
                }

                if (!w.world.chunks[x, z])
                {
                    // Chunk in range but never generated
                    w.world.chunks[x, z] = GenerateChunk(x, z, w.world, w.transform, isPlayerChunk);
                }
                else
                {
                    // Chunk in range but cached
                    if (!w.world.chunks[x, z].activeSelf)
                    {
                        w.world.chunks[x, z].SetActive(true);
                    }
                }
            }
        }

        yield break;
    }

    // Generates a brand new chunk that gets picked up by the ChunkSystem
    private GameObject GenerateChunk(int x, int z, World world, Transform transform, bool isPlayerChunk = false)
    {
        Vector3 position = transform.position;

        // Move chunks base position so [0, 0] is actually the center of the world and not the origin
        position.x += -((float)world.maxChunks * (float)world.chunkSize / 2);
        position.z += -((float)world.maxChunks * (float)world.chunkSize / 2);

        position.x += x * world.chunkSize;
        position.z += z * world.chunkSize;

        GameObject newChunk = GameObject.Instantiate(world.chunk, position, Quaternion.identity);
        newChunk.gameObject.GetComponent<VolumeChunk>().chunkSize = world.chunkSize;
        newChunk.gameObject.GetComponent<VolumeChunk>().needsUpdate = true;
        newChunk.gameObject.GetComponent<VolumeChunk>().isPlayerChunk = isPlayerChunk;

        newChunk.transform.parent = world.transform;

        return newChunk;
    }

    // Get current chunk that player is in
    private Vector2Int GetActiveChunk(int maxChunks, int chunkSize)
    {
        // Find current player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float playerX = player.transform.position.x;
        float playerZ = player.transform.position.z;

        // Move chunks base position so [0, 0] is actually the center of the world and not the origin
        playerX += chunkSize * maxChunks / 2f;
        playerZ += chunkSize * maxChunks / 2f;

        int x = Mathf.CeilToInt(playerX / chunkSize);
        int z = Mathf.CeilToInt(playerZ / chunkSize);

        // Chunk coordinates player is in
        return new Vector2Int(x - 1, z - 1);
    }
}
