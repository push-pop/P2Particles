using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Particles2))]
public class P2Renderer : MonoBehaviour
{
    public enum BlendMode
    {
        Additive,
        SoftAdditive,
        Alpha,
        Multiply
    };


    [SerializeField]
    protected Particles2 _particles;

    [SerializeField]
    protected Shader _shader;

    [SerializeField]
    protected Mesh _particleMesh;

    public Mesh ParticleMesh
    {
        get { return _particleMesh; }
        set { _particleMesh = value; }
    }

    public BlendMode _blendMode = BlendMode.Alpha;

    public Vector4 _remapFBM = new Vector4(0, 1, 0, 1);

    [Range(0, 1)]
    public float _fbmAmt = 1f;
    [Range(0, 2)]
    public float _fbmFreq = 1f;


    [Range(0, 1)]
    public float _particlize = 1.0f;
    [Range(0.001f, .5f)]
    public float _particleSize = .07f;
    [Range(0f,1f)]
    public float _softness;

    public Gradient ColorOverLife
    {
        get
        {
            return _colorOverLife;
        }
        set
        {
            _colorOverLife = value;
        }
    }
    [SerializeField]
    Gradient _colorOverLife = new Gradient();
    protected bool _rebuildGradient = false;

    public Texture2D GradientTex
    {
        get
        {
            if (_gradientTex == null || _rebuildGradient)
            {
                _gradientTex = _colorOverLife.ToTexture();
                _rebuildGradient = false;
            }

            return _gradientTex;
        }
    }
    Texture2D _gradientTex;

    [Range(0, 1)]
    public float _falloff = 1;
    [Range(0, 1)]
    public float _hueSpeed = 0;
    [Range(0, 100)]
    public float _renderLife = 4;
    public bool _cullLife = false;

    public bool _debugVelocity = false;

    protected Material _renderMaterial;

    protected ComputeBuffer _batchDrawArgs;
    protected ComputeBuffer _meshBuffer;
    public float _scaleOnTime;

    // Use this for initialization
    protected void Start()
    {
        Debug.Log("P2RendererStart");
        _renderMaterial = new Material(_shader);

        if (_particles == null)
            _particles = GetComponent<Particles2>();

        if (_particles == null)
        {
            this.enabled = false;
            return;
        }

        CreateDrawArgsBuffer();
        CreateSpriteMeshBuffer();
    }

    void CreateDrawArgsBuffer()
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        args[0] = (uint)_particles.NumParticles * ParticleMesh.GetIndexCount(0);
        args[1] = 1;

        Debug.Log("Drawing " + args[0] + " Verts");
        _batchDrawArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _batchDrawArgs.SetData(args);
    }

    void CreateSpriteMeshBuffer()
    {
        var meshDataArray = new MeshData[ParticleMesh.triangles.Length];
        for (int i = 0; i < ParticleMesh.vertices.Length; i++)
        {
            meshDataArray[i].vert = ParticleMesh.vertices[i];
        }
        for (int i = 0; i < ParticleMesh.uv.Length; i++)
        {
            meshDataArray[i].uv = ParticleMesh.uv[i];

        }
        for (int i = 0; i < ParticleMesh.triangles.Length; i++)
        {
            meshDataArray[i].index = ParticleMesh.triangles[i];
        }

        _meshBuffer = new ComputeBuffer(meshDataArray.Length, MeshData.stride);
        _meshBuffer.SetData(meshDataArray);

    }

    protected virtual void DoUpdateMaterialProperties()
    {

    }

    void UpdateMaterialProperties()
    {
        _renderMaterial.SetBuffer("Particles", _particles.ParticleBuffer);
        _renderMaterial.SetBuffer("meshData", _meshBuffer);
        _renderMaterial.SetInt("MeshIndexCount", (int)_particleMesh.triangles.Length);

        _renderMaterial.SetVector("objectPos", transform.position);
        _renderMaterial.SetFloat("_Scale", _particleSize);
        _renderMaterial.SetFloat("_Particlize", _particlize);
        _renderMaterial.SetMatrix("_ObjectTransform", Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, 1, 1)));
        _renderMaterial.SetTexture("_ColorOverLife", GradientTex);
        _renderMaterial.SetFloat("_Softness", _softness);
        _renderMaterial.SetFloat("_Falloff", _falloff);
        _renderMaterial.SetFloat("_HueSpeed", _hueSpeed);
        _renderMaterial.SetFloat("_NumParticles", _particles.NumParticles);
        _renderMaterial.SetFloat("_MaxLife", _renderLife);
        _renderMaterial.SetFloat("_FbmFreq", _fbmFreq);
        _renderMaterial.SetVector("_RemapFbm", _remapFBM);
        _renderMaterial.SetFloat("_FbmAmt", _fbmAmt);
        _renderMaterial.SetInt("_SrcMode", GetSrcMode());
        _renderMaterial.SetInt("_DstMode", GetDstMode());
        _renderMaterial.SetInt("_BlendEnum", (int)_blendMode);
        _renderMaterial.SetInt("_DebugVelocity", _debugVelocity ? 1 : 0);
        _renderMaterial.SetInt("_CullLife", _cullLife ? 1 : 0);
        _renderMaterial.SetPass(0);

        DoUpdateMaterialProperties();
    }

    private int GetSrcMode()
    {
        switch (_blendMode)
        {
            case BlendMode.Additive:
                return (int)UnityEngine.Rendering.BlendMode.One;
            case BlendMode.SoftAdditive:
                return (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor;
            case BlendMode.Alpha:
                return (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
            case BlendMode.Multiply:
                return (int)UnityEngine.Rendering.BlendMode.DstColor;
            default:
                return 0;
        }
    }

    private int GetDstMode()
    {
        switch (_blendMode)
        {
            case BlendMode.Additive:
                return (int)UnityEngine.Rendering.BlendMode.One;
            case BlendMode.SoftAdditive:
                return (int)UnityEngine.Rendering.BlendMode.One;
            case BlendMode.Alpha:
                return (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
            case BlendMode.Multiply:
                return (int)UnityEngine.Rendering.BlendMode.Zero;
            default:
                return 0;
        }
    }


    public void RenderParticles()
    {
        UpdateMaterialProperties();

        DoRenderParticles();
    }

    protected virtual void DoRenderParticles()
    {

    }

    private void OnRenderObject()
    {
        RenderParticles();
    }

}
