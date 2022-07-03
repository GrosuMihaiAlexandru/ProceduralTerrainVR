using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Chunk
{
	public GameObject chunkObject;
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	MeshRenderer meshRenderer;

	public Vector3Int chunkPosition;

	public List<Vector3> vertices = new List<Vector3>();
	public List<int> vertexTextureIndex = new List<int>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>(0);

	public int terrainWidth;
	public int terrainHeight;


	public TerrainPoint[,,] terrainMap;

	float minValue = float.MaxValue, maxValue = float.MinValue;

	public int width { get { return GameData.ChunkWidth; } }
	public int height { get { return GameData.ChunkHeight; } }
	float terrainSurface { get { return GameData.terrainSurface; } }

	private WorldGenerator worldGenerator;

	public Chunk(Vector3Int _position, WorldGenerator worldGenerator)
    {
		this.worldGenerator = worldGenerator;
		chunkObject = new GameObject();
		chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
		chunkPosition = _position;
		chunkObject.transform.position = chunkPosition;

		// Adding components to the chunk GO
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshCollider = chunkObject.AddComponent<MeshCollider>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
		meshRenderer.material.SetTexture("_TexArr", World.Instance.terrainTexArray);

		// Adding teleportation area
		chunkObject.AddComponent<TeleportationArea>();

		chunkObject.transform.tag = "Terrain";
		terrainMap = new TerrainPoint[width + 1, height + 1, width + 1];

		PopulateTerrainMap();
		CreateMeshData();
		BuildMesh();
    }

	/// <summary>
	/// Creates a 3D array of voxels for the terrain
	/// </summary>
	void PopulateTerrainMap()
    {
		for (int x = 0; x < width + 1; x++)
		{
			for (int z = 0; z < width + 1; z++)
			{
				// Getting a random texture for the current voxel
				// All voxels in a certain XZ will have the same texture regardless of Y
				int randomVoxel = Random.Range(0, World.Instance.terrainTextures.Length);

				for (int y = 0; y < height + 1; y++)
				{
					// get random height for the current XZ position
					float thisHeight = GameData.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z);

					// Debug.Log(thisHeight);

					// assign the height of the voxel based on Y position
					float point = (float)y - thisHeight;
					//Debug.Log("X:" + x + " Y:" + y + " Z:" + z + " Value: " + point);

					if (point > maxValue)
						maxValue = point;
					if (point < minValue)
						minValue = point;

					Chunk edgeChunk = null;

					// Checking the edge of the chunks to have consistent voxels types between chunks
					int edgeVoxel = 0;
					if (x == 0)
					{
						edgeChunk = worldGenerator.GetAdjacentChunk(this, new Vector2Int(-GameData.ChunkWidth, 0));
						if (edgeChunk != null)
							edgeVoxel = edgeChunk.terrainMap[width - 1, y, z].textureID;

					}
					else if (z == 0)
					{
						edgeChunk = worldGenerator.GetAdjacentChunk(this, new Vector2Int(0, -GameData.ChunkWidth));
						if (edgeChunk != null)
							edgeVoxel = edgeChunk.terrainMap[x, y, width - 1].textureID;
					}
					else if (x == width - 1)
					{
						edgeChunk = worldGenerator.GetAdjacentChunk(this, new Vector2Int(GameData.ChunkWidth, 0));
						if (edgeChunk != null)
							edgeVoxel = edgeChunk.terrainMap[0, y, z].textureID;
					}
					else if (z == width - 1)
					{
						edgeChunk = worldGenerator.GetAdjacentChunk(this, new Vector2Int(0, GameData.ChunkWidth));
						if (edgeChunk != null)
							edgeVoxel = edgeChunk.terrainMap[x, y, 0].textureID;
					}

					if (edgeChunk != null)
					{
						// get voxel on the edge of the chunk
						terrainMap[x, y, z] = new TerrainPoint(point, edgeVoxel);

					}
					else
					{
						terrainMap[x, y, z] = new TerrainPoint(point, randomVoxel);
					}
				}
			}
		}
        
    }

	/// <summary>
	/// Creating the mesh of the chunk
	/// </summary>
	void CreateMeshData()
    {
		ClearMeshData();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					float[] cube = new float[8];
					for (int i = 0; i < 8; i++)
                    {
						Vector3Int corner = new Vector3Int(x, y, z) + GameData.CornerTable[i];
						cube[i] = terrainMap[corner.x, corner.y, corner.z].dstToSurface;
                    }

					MarchCube(new Vector3(x, y, z), cube);
				}
			}
		}
		BuildMesh();
	}

	// Determine the index into the edge table which tells us which vertices are inside of the surface
	int FindVerticesInsideSurface(float[] cube)
	{
		int configurationIndex = 0;

		for (int i = 0; i < 8; i++)
        {
			if (cube[i] > terrainSurface)
            {
				configurationIndex |= 1 << i;
            }
        }

		return configurationIndex;
	}

    void MarchCube(Vector3 position, float[] cube)
    {
		int configIndex = FindVerticesInsideSurface(cube); 

		if (configIndex == 0 || configIndex == 255)
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
        {
			for (int j = 0; j < 3; j++)
            {
				int indice = GameData.TriangleTable[configIndex, edgeIndex];

				if (indice == -1)
					return;

				Vector3 vert1 = position + (Vector3)GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + (Vector3)GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

				Vector3 vertPosition;

				// Get the terrain values at either end of the current edge from the cube array
				float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
				float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

				// Calculate the differencce between the terrain values
				float difference = vert2Sample - vert1Sample;

				// if the difference is 0, then the terrain passes through the middle
				if (difference == 0)
					difference = terrainSurface;
				else
					difference = (terrainSurface - vert1Sample) / difference;

				// Calculate the point along the edge that passes through
				vertPosition = vert1 + ((vert2 - vert1) * difference);


				triangles.Add(VertForIndice(vertPosition, position));

				edgeIndex++;
            }
        }
    }

	// Placing a voxel in the chunk
	public void PlaceTerrain(Vector3 pos)
    {
		Vector3Int intPos = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y) , Mathf.CeilToInt(pos.z));
		intPos -= chunkPosition;
		Debug.Log(intPos);
		terrainMap[intPos.x, intPos.y, intPos.z].dstToSurface = 0f;

		CreateMeshData();

    }

	// Removing a voxel from the chunk
	public void RemoveTerrain(Vector3 pos)
    {
		Vector3Int intPos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		intPos -= chunkPosition;
		Debug.Log(intPos);
		terrainMap[intPos.x, intPos.y, intPos.z].dstToSurface = 1f;

		CreateMeshData();
	}

	// Check that every vertice is added only once
	int VertForIndice(Vector3 vert, Vector3 position)
    {
		for (int i = 0; i < vertices.Count; i++)
        {
			if (vertices[i] == vert)
				return i;
        }

		vertices.Add(vert);

		int textureId = terrainMap[(int)(position.x), (int)(position.y), (int)(position.z)].textureID;
		uvs.Add(new Vector2(textureId, 0));
		vertexTextureIndex.Add(textureId);
		//colors.Add(TerrainPoint.colors[terrainMap[(int)(position.x * resolution), (int)(position.y * resolution), (int)(position.z * resolution)].textureID]);

		return vertices.Count - 1;
    }

    void ClearMeshData()
    {
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
    }

	void BuildMesh()
    {
		Debug.Log(vertices.Count);
        Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		//mesh.colors = colors.ToArray();
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		meshCollider.sharedMesh = mesh;
    }

}
