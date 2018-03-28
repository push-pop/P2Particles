using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MeshData
{
    //32 length arrays declared in shader

    public Vector3 vert;
    public Vector2 uv;
    public int index;
    public Vector3 norm;

    public const int stride = 8 * sizeof(float) + sizeof(int);
}
