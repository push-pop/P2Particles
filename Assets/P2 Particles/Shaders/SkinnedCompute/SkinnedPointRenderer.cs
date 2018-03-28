using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedPointRenderer : MonoBehaviour {

    [SerializeField] SkinnedPointSource _pointSource;
    [SerializeField] Shader _shader;

    private Material _mat;

	// Use this for initialization
	void Start () {
        _mat = new Material(_shader);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnRenderObject()
    {
        _mat.SetBuffer("_PointBuffer", _pointSource.BakedPoints);
        _mat.SetPass(0);

        Graphics.DrawProcedural(MeshTopology.Triangles, _pointSource.VertexCount, 1);
    }
}
