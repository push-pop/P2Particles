using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[ExecuteInEditMode]
public class Particles2 : MonoBehaviour
{

    #region Compute

    [Serializable]
    public struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector2 uv;
        public float life;
        public float age;

        public const int stride = 10 * sizeof(float);

    };

    [Serializable]
    public struct SystemInfo
    {
        public int emitterIndex;

        public const int stride = sizeof(int);
    };


    public enum Kernels
    {
        UpdateParticles,
        UpdateLorenz,
        UpdateVelocity,
        UpdateRotation,
        InitParticles
    }

    [SerializeField]
    ComputeShader ComputeKernels;
    //Instance
    private ComputeShader _kernels;
     
    public Dictionary<Kernels, int> kernelDictionary;

    public ComputeBuffer ParticleBuffer
    {
        get {
            if (_particleBuffer == null)
                Debug.Log("Particle buffer is null?");

            return _particleBuffer;
        }
    }
    public ComputeBuffer MeshBuffer
    {
        get { return _meshBuffer; }
    }

    ComputeBuffer _particleBuffer;
    ComputeBuffer _meshBuffer;
    ComputeBuffer _batchDrawArgs;
    ComputeBuffer _vFieldInfo;
    ComputeBuffer _systemInfoBuffer;
    #endregion

    #region Properties
    public int NumParticles
    {
        get { return _numParticles; }
    }

    [SerializeField]
    int _numParticles = 5000;

    [SerializeField]
    SkinnedPointSource _skinnedEmitter;

    public bool _restartSimulation = false;

    [SerializeField]
    VectorField _vectorField;

    [Range(0, 1)]
    public float _vectorFieldFollow;

    [SerializeField]
    Texture2D _uvParam1;

    [SerializeField]
    Texture2D _uvParam2;

    [Range(-10, 10)]
    public float _centerAttractorMult;

    [Range(-20, 20)]
    public float _noiseAmplitude;

    [Range(0, 10)]
    public float _noiseFrequency = 0.25f;

    [Range(0, 1)]
    public float _damping = 0.08f;

    [Range(0, 10)]
    public float _sphereRadius = 1;

    [Range(0, 1)]
    public float _maxLife = 0.5f;

    public bool doUpdate = true;

    //Private Variables
    private Vector2 _uvStep;

    #endregion

    #region Private Methods

    private void CreateKernelDictionary()
    {
        kernelDictionary = new Dictionary<Kernels, int>();
        kernelDictionary.Add(Kernels.InitParticles, _kernels.FindKernel("InitParticles"));
        kernelDictionary.Add(Kernels.UpdateParticles, _kernels.FindKernel("UpdateParticles"));
    }

    private void InitializeParticles()
    {
        UpdateComputeParameters();

        _kernels.SetBuffer(kernelDictionary[Kernels.InitParticles], "Particles", _particleBuffer);
        _kernels.SetBuffer(kernelDictionary[Kernels.InitParticles], "Info", _systemInfoBuffer);
        _kernels.Dispatch(kernelDictionary[Kernels.InitParticles], _numParticles, 1, 1);
    }

    private void UpdateParticleSystem()
    {
        BindComputeShaderBuffers();
        UpdateComputeParameters();

        _kernels.Dispatch(kernelDictionary[Kernels.UpdateParticles], _numParticles, 1, 1);

    }

    private void UpdateComputeParameters()
    {
        if (_vectorField != null && _vectorField.isActiveAndEnabled)
        {
            if (_vFieldInfo == null)
                _vFieldInfo = new ComputeBuffer(1, VectorField.FieldInfo.stride);

            var info = new VectorField.FieldInfo[] { _vectorField.Info };

            _vFieldInfo.SetData(info);

            if (_vectorField.FieldTexture != null)
                _kernels.SetTexture(kernelDictionary[Kernels.UpdateParticles], "_vectorField", _vectorField.FieldTexture);

            _kernels.SetBuffer(kernelDictionary[Kernels.UpdateParticles], "_vFieldInfo", _vFieldInfo);
            _kernels.SetMatrix("_transform", Matrix4x4.TRS(_vectorField.transform.position, _vectorField.transform.rotation, _vectorField.transform.localScale));
        }

        _kernels.SetInt("_emitterCount", _skinnedEmitter.VertexCount);
        _kernels.SetVector("_sphereCenter", transform.position);

        _kernels.SetFloat("damping", _damping);
        _kernels.SetFloat("_sphereRadius", _sphereRadius);
        _kernels.SetFloat("_numParticles", _numParticles);
        _kernels.SetFloat("_dt", Time.deltaTime);
        _kernels.SetFloat("_maxLife", _maxLife);
        _kernels.SetFloat("_noiseAmplitude", _noiseAmplitude);
        _kernels.SetFloat("_centerAttractorMult", _centerAttractorMult);
        _kernels.SetFloat("_noiseFrequency", _noiseFrequency);
        _kernels.SetFloat("_vectorFieldFollow", _vectorFieldFollow);
        _kernels.SetVector("_uvStep", _uvStep);


        if (_skinnedEmitter != null && _skinnedEmitter.BakedPoints != null)
            _kernels.SetBuffer(kernelDictionary[Kernels.UpdateParticles], "SkinnedPoints", _skinnedEmitter.BakedPoints);

        _kernels.SetBuffer(kernelDictionary[Kernels.UpdateParticles], "Particles", _particleBuffer);
    }

    private void CreateBuffers()
    {
        _particleBuffer = new ComputeBuffer(_numParticles, Particle.stride);
        _systemInfoBuffer = new ComputeBuffer(1, SystemInfo.stride);
    }

    private void BindComputeShaderBuffers()
    {

        _kernels.SetBuffer(kernelDictionary[Kernels.UpdateParticles], "Info", _systemInfoBuffer);
        _kernels.SetBuffer(kernelDictionary[Kernels.UpdateParticles], "Particles", _particleBuffer);
        _kernels.SetTexture(kernelDictionary[Kernels.UpdateParticles], "_UVParam1", _uvParam1);
        _kernels.SetTexture(kernelDictionary[Kernels.UpdateParticles], "_UVParam2", _uvParam2);
    }

    private void DestroyBuffers()
    {
        if (_particleBuffer != null)
            _particleBuffer.Release();
        if (_batchDrawArgs != null)
            _batchDrawArgs.Release();
        if (_vFieldInfo != null)
            _vFieldInfo.Release();
        if (_meshBuffer != null)
            _meshBuffer.Release();
    }

    #endregion

    #region Unity Methods
    void Start()
    {
        var sqr = Mathf.CeilToInt(Mathf.Sqrt(_numParticles));
        _numParticles = sqr * sqr;
        _uvStep = new Vector2(1.0f / (float)sqr, 1.0f / (float)sqr);

        _kernels = Instantiate(ComputeKernels) as ComputeShader;

        if (_uvParam1 == null)
            _uvParam1 = Texture2D.whiteTexture;
        if (_uvParam2 == null)
            _uvParam2 = Texture2D.whiteTexture;

        CreateKernelDictionary();

        CreateBuffers();

        BindComputeShaderBuffers();

        InitializeParticles();

    }

    void OnDestroy()
    {
        DestroyBuffers();
    }

    void FixedUpdate()
    {
        if (_restartSimulation)
        {
            _particleBuffer.Release();
            _particleBuffer = null;
            _restartSimulation = false;
        }

        if (_particleBuffer == null)
        {
            CreateBuffers();
            BindComputeShaderBuffers();

            InitializeParticles();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            _restartSimulation = true;

        if (doUpdate)
            UpdateParticleSystem();
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sphereRadius);

        if (_vectorField != null)
            _vectorField.DrawGizmos();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_kernels == null)
        {
            AssetDatabase.FindAssets("ParticleKernels").ToList().ForEach(x =>
            {
                var shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath(x));
                if (shader != null)
                {
                    ComputeKernels = shader;
                    return;
                }

            });
        }
    }
#endif

    #endregion
}
