using System.Collections;
using UnityEngine;
using Unity.Entities;

public class World : MonoBehaviour
{
    public GameObject chunk;
    public int renderDistance = 5;
    public int maxChunks = 3;
    public int chunkSize = 3;

    public GameObject[,] chunks;
    public bool initialised = false;
}

public class WorldSystem : ComponentSystem
{


    struct WorldGenerator
    {
        public World world;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var w in GetEntities<WorldGenerator>())
        {
            if (!w.world.initialised)
            {
                w.world.chunks = new GameObject[w.world.maxChunks, w.world.maxChunks];
                w.world.initialised = true;
            }

            Vector2Int playerChunk = GetActiveChunk(w.world.maxChunks, w.world.chunkSize);

            IEnumerator coroutine = ChunkGenerator(w, playerChunk);
            w.world.StartCoroutine(coroutine);
        }
    }

    IEnumerator ChunkGenerator(WorldGenerator w, Vector2Int playerChunk)
    {
        for (int x = 0; x < w.world.maxChunks; x++)
        {
            for (int y = 0; y < w.world.maxChunks; y++)
            {
                bool isInX = playerChunk.x - w.world.renderDistance < x && playerChunk.x + w.world.renderDistance > x;
                bool isInY = playerChunk.y - w.world.renderDistance < y && playerChunk.y + w.world.renderDistance > y;
                bool isPlayerChunk = (playerChunk.x == x && playerChunk.y == y);

                if (!isInX || !isInY)
                {
                    if (w.world.chunks[x, y])
                    {
                        w.world.chunks[x, y].SetActive(false);
                    }

                    continue;
                }

                if (!w.world.chunks[x, y])
                {
                    w.world.chunks[x, y] = GenerateChunk(x, y, w.world, w.transform, isPlayerChunk);
                }
                else
                {
                    if (!w.world.chunks[x, y].activeSelf)
                    {
                        w.world.chunks[x, y].SetActive(true);
                    }
                }
               

            }

            //yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    private GameObject GenerateChunk(int x, int y, World world, Transform transform, bool isPlayerChunk = false)
    {
        Vector3 position = transform.position;

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

    private Vector2Int GetActiveChunk(int maxChunks, int chunkSize)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float playerX = player.transform.position.x;
        float playerY = player.transform.position.z;

        playerX += chunkSize * maxChunks / 2f;
        playerY += chunkSize * maxChunks / 2f;

        int x = Mathf.CeilToInt(playerX / chunkSize);
        int y = Mathf.CeilToInt(playerY / chunkSize);

        return new Vector2Int(x - 1, y - 1);
    }
}
