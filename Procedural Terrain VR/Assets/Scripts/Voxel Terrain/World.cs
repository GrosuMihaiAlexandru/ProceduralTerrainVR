using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance;

    public Texture2D[] terrainTextures;
    public Texture2DArray terrainTexArray;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        PopulateTextureArray();
    }

    void PopulateTextureArray()
    {
        terrainTexArray = new Texture2DArray(1024, 1024, terrainTextures.Length, TextureFormat.ARGB32, false);

        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTexArray.SetPixels(terrainTextures[i].GetPixels(0), i, 0);
        }
        terrainTexArray.Apply();
    }

}
