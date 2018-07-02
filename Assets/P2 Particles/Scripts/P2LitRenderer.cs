using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Particles2))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class P2LitRenderer : P2Renderer
{
    public Texture2D BumpMap;
    public float BumpAmt;

    public Mesh _dummyMesh;

    public Vector3 _xyzScale = Vector3.one;

    // Use this for initialization
    new void Start()
    {
        base.Start();

        _renderMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        CreateDummyMesh();
    }

    void CreateDummyMesh()
    {
        var totalVerts = _particles.NumParticles * _particleMesh.vertexCount;

        var verts = new Vector3[totalVerts];
        var indices = new int[totalVerts];
        var uvs = new Vector2[totalVerts];

        Debug.Log("Total Verts: " + totalVerts);

        _dummyMesh = new Mesh();

        for (int i = 0; i < totalVerts; i++)
        {
            indices[i] = i;
            verts[i] = Vector3.zero;
            uvs[i] = Vector2.zero;
        }

        _dummyMesh.name = string.Format("Dummy mesh with {0} verts", totalVerts);
        _dummyMesh.vertices = verts;
        _dummyMesh.indexFormat = IndexFormat.UInt32;
        _dummyMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        _dummyMesh.bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));

        GetComponent<MeshFilter>().mesh = _dummyMesh;

    }

    protected override void DoUpdateMaterialProperties()
    {
        _renderMaterial.SetTexture("_BumpMap", BumpMap);
        _renderMaterial.SetFloat("_BumpScale", BumpAmt);
        _renderMaterial.SetVector("_XYZScale", _xyzScale);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _rebuildGradient = true;
        if (_particles == null)
            _particles = GetComponent<Particles2>();

        if (_shader == null)
            _shader = Shader.Find("P2/LitParticles");

        if (_particleMesh == null)
        {
            // Any better way to do this ??
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _particleMesh = g.GetComponent<MeshFilter>().sharedMesh;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(g);
            };
        }
    }
#endif
}
