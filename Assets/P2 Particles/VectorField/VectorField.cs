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
            return _vField.Field;
        }
    }

    public FieldInfo Info
    {
        get { return _fieldInfo; }

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

    [SerializeField]
    TextAsset _fieldAsset;

    [SerializeField]
    TextureWrapMode _textureWrapMode = TextureWrapMode.Clamp;

    [SerializeField]
    bool _swapYZ = true;

    VectorFieldTexture _vField;
    void Awake()
    {
        FieldToLoad f = new FieldToLoad()
        {
            swapYZ = _swapYZ,
            wrapMode = _textureWrapMode,
            textAssset = _fieldAsset
        };

        _vField = VectorFieldLoader.Instance.Load(f);

        if (_vField == null)
        {
            enabled = false;
            return;
        }

        _fieldInfo.Resolution = _vField._info.Resolution;
        _fieldInfo.BoundingMaximum = _vField._info.BoundingMaximum;
        _fieldInfo.BoundingMinimum = _vField._info.BoundingMinimum;
        _fieldInfo.FieldScale = _fieldScale;
        _fieldInfo.ForceScale = _forceScale;
    }

    // Update is called once per frame
    void Update()
    {
        _fieldInfo.Center = transform.position;
        _fieldInfo.FieldScale = _fieldScale;
        _fieldInfo.ForceScale = _forceScale;
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
