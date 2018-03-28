using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedPointSource : MonoBehaviour
{
    public ComputeBuffer BakedPoints
    {
        get { return _bakedPoints; }
    }

    public int VertexCount
    {
        get { return _meshData.vertexCount; }
    }

    [SerializeField] SkinnedMeshData _meshData;

    Camera _cam;
    ComputeBuffer _bakedPoints;
    SkinnedMeshRenderer _target;
    Material _placeholderMaterial;
    Shader _replacementShader;

    public bool RenderBody = false;

    // Use this for initialization
    void Awake()
    {
        CreateBuffer();
        BuildCamera();
        OverrideRenderer();
    }

    private void OverrideRenderer()
    {
        var smr = GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = _meshData.mesh;
        smr.material = _placeholderMaterial;
        smr.receiveShadows = false;

        // This renderer is disabled to hide from other cameras. It will be
        // enable by CullingStateController only while rendered from our
        // vertex baking camera.
        smr.enabled = false;
    }

    private void CreateBuffer()
    {
        _bakedPoints = new ComputeBuffer(_meshData.vertexCount, PointData.stride);
        Debug.Log("Created Compute Buffer for " + _meshData.vertexCount + " vertices");
    }

    private void BuildCamera()
    {
        var go = new GameObject("Camera");
        go.hideFlags = HideFlags.HideInHierarchy;

        var tr = go.transform;
        tr.parent = transform;
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;

        _cam = go.AddComponent<Camera>();

        _cam.renderingPath = RenderingPath.Forward;
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.depth = -10000;

        _cam.nearClipPlane = -100;
        _cam.farClipPlane = 100;
        _cam.orthographic = true;
        _cam.orthographicSize = 100;

        _cam.enabled = false;

        _target = GetComponent<SkinnedMeshRenderer>();
    }

    void LateUpdate()
    {
        BakeSkinnedMesh();
    }

    private void BakeSkinnedMesh()
    {
        _target.enabled = true;

        Graphics.SetRandomWriteTarget(1, BakedPoints, true);
        _cam.RenderWithShader(_replacementShader, "P2Replacement");
        Graphics.ClearRandomWriteTargets();

        _target.enabled = RenderBody;
    }

    private void OnValidate()
    {
        _placeholderMaterial = new Material(Shader.Find("P2/Placeholder"));
        _replacementShader = Shader.Find("P2/Replacement");
    }

}
