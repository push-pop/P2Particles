using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Particles2))]
public class P2UnlitRenderer : P2Renderer
{

    new void Start()
    {
        base.Start();
    }

    protected override void DoUpdateMaterialProperties()
    {

    }

    protected override void DoRenderParticles()
    {
        Graphics.DrawProceduralIndirect(MeshTopology.Triangles, _batchDrawArgs);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_particles == null)
            _particles = GetComponent<Particles2>();

        if (_shader == null)
            _shader = Shader.Find("P2/UnlitParticles");

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
#endif
}
