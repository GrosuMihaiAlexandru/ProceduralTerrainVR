using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPoint
{
    public static Color[] colors = new Color[3] { Color.red, Color.green, Color.blue };

    public float dstToSurface = 1f;

    public int textureID = 0;

    public TerrainPoint(float dst, int tex)
    {
        dstToSurface = dst;
        textureID = tex;
    }
}
