using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Particles2))]
public class P2LitRenderer : P2Renderer
{
    public Gradient _colorOverLife = new Gradient();

    public Texture2D GradientTex
    {
        get
        {
            if (_gradientTex == null)
                _gradientTex = _colorOverLife.ToTexture();
            return _gradientTex;
        }
    }
    Texture2D _gradientTex;

    public Texture2D BumpMap;
    public float BumpAmt;


    public Mesh _dummyMesh;

    // Use this for initialization
    new void Start()
    {
        base.Start();
         
        CreateDummyMesh();
    }

    void CreateDummyMesh()
    {
        _dummyMesh = new Mesh();
        var totalVerts = _particles.NumParticles * _particleMesh.triangles.Length;
        var verts = new Vector3[totalVerts];
        var indices = new int[totalVerts];
        var uvs = new Vector2[totalVerts];

        Debug.Log("Total Verts: " + totalVerts);

        for (int i = 0; i < totalVerts; i++)
        {
            indices[i] = i;
            verts[i] = Vector3.zero;
            uvs[i] = Vector2.zero;
        }
        _dummyMesh.name = totalVerts.ToString();
        _dummyMesh.vertices = verts;
        _dummyMesh.indexFormat = IndexFormat.UInt32;
        _dummyMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        _dummyMesh.bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));

    }
    
    protected override void DoUpdateMaterialProperties()
    {
       // _renderMaterial.SetTexture("_BumpMap", BumpMap);
        //_renderMaterial.SetFloat("_BumpScale", BumpAmt);

//        _renderMaterial.SetPass(0);
    }

    protected override void DoRenderParticles()
    {
        Graphics.DrawMeshInstancedIndirect(_dummyMesh, 0, _renderMaterial, _dummyMesh.bounds, _batchDrawArgs, 0, null, ShadowCastingMode.On, true, gameObject.layer, Camera.main);
    }

    private void OnValidate()
    {
        if (_particles == null)
            _particles = GetComponent<Particles2>();

        if (_shader == null)
            _shader = Shader.Find("P2/LitParticles");

        if (_particleMesh == null)
        {
            // Any better way to do this ??
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _particleMesh = g.GetComponent<MeshFilter>().sharedMesh;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(g);
            };
        }
    }
}
