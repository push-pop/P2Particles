using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GradientTex
{
    public Gradient _grad = GradientExt.DefaultGradient;
    public TextureWrapMode _wrapMode;
    public Texture2D Texture
    {
        get;
        set;
    }

    public void BakeTexture()
    {

    }
}

public static class GradientExt
{
    public static Gradient DefaultGradient
    {
        get
        {
            var g = new Gradient();
            g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.cyan, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0f) }
            );

            return g;
        }
    }

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
