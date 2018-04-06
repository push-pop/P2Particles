using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GradientExt
{
    public static Texture2D ToTexture(this Gradient g, FilterMode f = FilterMode.Point, TextureWrapMode w = TextureWrapMode.Clamp)
    {
        Texture2D t = new Texture2D(256, 1, TextureFormat.RGBA32, false);

        t.filterMode = f;
        t.wrapMode = w;

        float step = 1f / (float)t.width;
        float s = 0;

        for (int u = 0; u < t.width; u++, s += step)
            t.SetPixel(u, 0, g.Evaluate(s));

        t.Apply();

        return t;
    }
}
