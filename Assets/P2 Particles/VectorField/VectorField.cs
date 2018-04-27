using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public struct FieldToLoad
{
    public TextAsset textAssset;
    public TextureWrapMode wrapMode;
    public bool swapYZ;
}

public class VectorField : MonoBehaviour
{
    public RenderTexture FieldTexture
    {
        get
        {
            return _dynamic ? _dynamicField.Field : _vField.Field;
        }
    }

    public FieldInfo Info
    {
        get
        {
            return _dynamic ? _dynamicFieldInfo : _fieldInfo;
        }

    }

    [System.Serializable]
    public struct FieldInfo
    {
        public Vector3 Center;
        public Vector3 Resolution;
        public Vector3 BoundingMinimum;
        public Vector3 BoundingMaximum;


        public float FieldScale;
        public float ForceScale;

        public static int stride = 14 * sizeof(float);
    }


    [Range(.001f, 10)]
    public float _fieldScale = 1f;

    [Range(-10, 10)]
    public float _forceScale = 1f;

    FieldInfo _fieldInfo;

    FieldInfo _dynamicFieldInfo;

    [SerializeField]
    TextAsset _fieldAsset;

    [SerializeField]
    TextureWrapMode _textureWrapMode = TextureWrapMode.Clamp;

    [SerializeField]
    bool _swapYZ = true;

    [SerializeField]
    bool _dynamic = false;

    [SerializeField]
    int _dynamicFieldResolution = 10;

    [SerializeField]
    List<FieldInfluencer> _fieldInfluencers = new List<FieldInfluencer>();

    [SerializeField]
    float _damping = 0.5f;

    ComputeShader _updateDynamicField;
    ComputeBuffer _influencerBuffer;
    ComputeBuffer _fieldInfoBuffer;

    [SerializeField]
    VectorFieldTexture _vField;
    [SerializeField]
    VectorFieldTexture _dynamicField;

    [SerializeField]
    bool _clearDynamic = false;

    void OnEnable()
    {
        if (_updateDynamicField == null)
            _updateDynamicField = Resources.Load<ComputeShader>("UpdateVectorField");



        if (_fieldAsset == null)
            CreateEmptyField();
        else
        {
            FieldToLoad f = new FieldToLoad()
            {
                swapYZ = _swapYZ,
                wrapMode = _textureWrapMode,
                textAssset = _fieldAsset
            };

            _vField = VectorFieldLoader.Instance.Load(f);
        }

        _fieldInfo.Resolution = _vField._info.Resolution;
        _fieldInfo.BoundingMaximum = _vField._info.BoundingMaximum;
        _fieldInfo.BoundingMinimum = _vField._info.BoundingMinimum;
        _fieldInfo.FieldScale = _fieldScale;
        _fieldInfo.ForceScale = _forceScale;

        _fieldInfoBuffer = new ComputeBuffer(1, VectorField.FieldInfo.stride);
        _fieldInfoBuffer.SetData(new VectorField.FieldInfo[]
        {
            _fieldInfo
        });

        if (_dynamic)
        {
            _dynamicFieldInfo = new FieldInfo()
            {
                Resolution = new Vector3(_dynamicFieldResolution, _dynamicFieldResolution, _dynamicFieldResolution),
                BoundingMaximum = new Vector3(1, 1, 1),
                BoundingMinimum = new Vector3(-1, -1, -1),
                Center = Vector3.zero,
                FieldScale = 1,
                ForceScale = 1
            };

            CreateDynamicField();
            CreateInfluencerBuffer();

            if (_dynamicField.hasLoaded)
            {
                _updateDynamicField.SetTexture(0, "_dynamic", _dynamicField.Field);
                _updateDynamicField.SetTexture(1, "_dynamic", _dynamicField.Field);
            }


            if (_vField.hasLoaded)
            {
                _updateDynamicField.SetTexture(1, "_baseField", _vField.Field);
            }

            _updateDynamicField.SetBuffer(0, "_vFieldInfo", _fieldInfoBuffer);
        }



    }

    private void CreateInfluencerBuffer()
    {
        if (_fieldInfluencers.Count > 0)
        {
            _influencerBuffer = new ComputeBuffer(_fieldInfluencers.Count, FieldInfluencerData.stride);

            _updateDynamicField.SetBuffer(0, "_influencers", _influencerBuffer);
            _updateDynamicField.SetInt("_influencerCount", _fieldInfluencers.Count);
        }
    }

    private void CreateDynamicField()
    {
        _dynamicField.Field = new RenderTexture(10, 10, 10, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
        {
            name = "Dynamic Field",
            filterMode = FilterMode.Trilinear,
            wrapMode = _textureWrapMode,
            volumeDepth = 10,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true
        };

        _dynamicField._info = new VectorFieldLoader.FieldInfo()
        {
            Resolution = new Vector3(10, 10, 10),
            BoundingMaximum = new Vector3(1, 1, 1),
            BoundingMinimum = new Vector3(-1, -1, -1),

        };

        _dynamicField.hasLoaded = _dynamicField.Field.Create();
    }

    private void CreateEmptyField()
    {
        _vField.Field = new RenderTexture(10, 10, 10, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
        {
            name = "Empty Field",
            filterMode = FilterMode.Trilinear,
            wrapMode = _textureWrapMode,
            volumeDepth = 10,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true
        };

        _vField._info = new VectorFieldLoader.FieldInfo()
        {
            Resolution = new Vector3(10, 10, 10),
            BoundingMaximum = new Vector3(1, 1, 1),
            BoundingMinimum = new Vector3(-1, -1, -1),
        };

        _vField.hasLoaded = _vField.Field.Create();
    }

    // Update is called once per frame
    void Update()
    {
        _fieldInfo.Center = transform.position;
        _fieldInfo.FieldScale = _fieldScale;
        _fieldInfo.ForceScale = _forceScale;
        _vField.Field.wrapMode = _textureWrapMode;

        if (_dynamic)
        {
            _dynamicFieldInfo.Center = transform.position;
            _dynamicFieldInfo.FieldScale = _fieldScale;
            _dynamicFieldInfo.ForceScale = _forceScale;
            _dynamicField.Field.wrapMode = _textureWrapMode;

            _dynamicField.Field.wrapMode = _textureWrapMode;
            if (_fieldInfluencers.Count > 0)
                _influencerBuffer.SetData(_fieldInfluencers.Select(e => e.Data).ToList());

            _fieldInfoBuffer.SetData(new VectorField.FieldInfo[]
            {
            _fieldInfo
                });

            _updateDynamicField.SetBuffer(0, "_influencers", _influencerBuffer);
            _updateDynamicField.SetBuffer(0, "_vFieldInfo", _fieldInfoBuffer);
            _updateDynamicField.SetInt("_texRes", (int)(_fieldInfo.FieldScale * _fieldInfo.Resolution.x));
            _updateDynamicField.SetFloat("_dt", Time.deltaTime);
            _updateDynamicField.SetFloat("_damping", _clearDynamic ? 1 : _damping);
            _updateDynamicField.Dispatch(0, 10 * 10 * 10, 1, 1);
            _updateDynamicField.Dispatch(1, 10 * 10 * 10, 1, 1);

            _clearDynamic = false;
        }
    }

    public void DrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = Color.yellow;

        if (Application.isPlaying)
            Gizmos.DrawWireCube(_fieldInfo.Center, _fieldScale * (_fieldInfo.BoundingMaximum - _fieldInfo.BoundingMinimum));
        else
            Gizmos.DrawWireCube(transform.position, 2 * new Vector3(_fieldScale, _fieldScale, _fieldScale));
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

}
