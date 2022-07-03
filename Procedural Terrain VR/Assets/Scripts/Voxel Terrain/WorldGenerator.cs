using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks = 10;

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    void Generate()
    {
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth, 0, z * GameData.ChunkWidth);
                chunks.Add(chunkPos, new Chunk(chunkPos, this));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];

    }

    public Chunk GetAdjacentChunk(Chunk chunk, Vector2Int direction)
    {
        var chunkPosition = chunk.chunkPosition + new Vector3Int(direction.x, 0, direction.y);
        if (chunks.ContainsKey(chunkPosition))
            return chunks[chunkPosition];
        else
            return null;
    }

    private void OnDrawGizmosSelected()
    {
        Chunk chunk = chunks[Vector3Int.zero];
        Chunk chunk2 = chunks[new Vector3Int(0, 0, GameData.ChunkWidth)];

        for (int i = 0; i < chunk.vertices.Count; i++)
        {
            Gizmos.color = TerrainPoint.colors[chunk.vertexTextureIndex[i]];
            Gizmos.DrawSphere(chunk.vertices[i], .1f);
        }

        for (int x = 0; x < chunk.width; x++)
        {
            for (int y = 0; y < chunk.height; y++)
            {
                for (int z = 0; z < chunk.width; z++)
                {
                    Gizmos.color = TerrainPoint.colors[chunk.terrainMap[x,y,z].textureID];
                    Gizmos.DrawSphere(new Vector3(x, y, z), 0.1f);
                }
            }
        }

        for (int x = 0; x < chunk2.width; x++)
        {
            for (int y = 0; y < chunk2.height; y++)
            {
                for (int z = 0; z < chunk2.width; z++)
                {
                    Gizmos.color = TerrainPoint.colors[chunk2.terrainMap[x, y, z].textureID];
                    Gizmos.DrawSphere(new Vector3(x , y, z + GameData.ChunkWidth), 0.1f);
                }
            }
        }
    }
}
