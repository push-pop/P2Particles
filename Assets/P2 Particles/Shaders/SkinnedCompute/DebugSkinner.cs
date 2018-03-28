using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSkinner : MonoBehaviour
{

    [SerializeField] SkinnedPointSource _source;
    [SerializeField] Shader _shader;
    Material mat;

    

    // Use this for initialization
    void Start()
    {
        mat = new Material(_shader);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        mat.SetBuffer("_PointBuffer", _source.BakedPoints);
        mat.SetInt("_VertexCount", _source.VertexCount);
        Graphics.Blit(source, destination, mat);
        //Graphics.Blit(_source.t, destination);
    }
}
