using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

public class runShader2 : MonoBehaviour {

	public int amount;
	public ComputeShader computeShader;
	int kernel = 0;
    public Color color = Color.white;

	public Texture spriteTexture;
	public float size;

    public Vector3 NoiseSpeed;
    public Vector3 NoiseAmount;
    public Vector3 NoiseFrequency;
    
	public Shader renderShader;
	Material renderMaterial;

	Vec[] data;// = new Vec[16];
	Vec[] output;// = new Vec[16];


	public Mesh ParticleMesh;
	Mesh _dummyMesh;

	ComputeBuffer buffer;
	ComputeBuffer _batchDrawArgs;
	ComputeBuffer _meshBuffer;
	//ComputeBuffer _particleBuffer;

	public bool debug;

	public GameObject Left;
	public GameObject Right;

    public float LeftSpeed;
    public float RightSpeed;

	Vector3 LeftPrev;
	Vector3 RightPrev;
    

	struct Vec
	{
		public Vector3 pos;
        public Vector3 nPos;
        public Vector3 pPos;
        public Vector3 vel;

        public const int stride = 12 * sizeof(float);
	}

	void Start(){
		
		CreateDummyMesh ();
		CreateDrawArgsBuffer();
		CreateSpriteMeshBuffer();
		SetupComputeShader ();
		Debug.Log (buffer.count);
       

	}

	void Update(){
		
		if(debug){
			buffer.GetData (output);
			for (int i = 0; i < output.Length; i++) {
				Debug.Log (output [i].pos);
			}
		}

		UpdateShaderProperties();
		UpdateMaterialProperties ();
		RenderParticles ();
	}

	void SetupComputeShader()
	{
		
		data = new Vec[amount];
		output = new Vec[amount];
		renderMaterial = new Material(renderShader);

		if (spriteTexture != null) {
			renderMaterial.SetTexture ("_MainTex", spriteTexture);
		}

		for (int i = 0; i < amount; i++) {
            data[i].pos = new Vector3(Mathf.Sin(((float)i / amount) * Mathf.PI * 200), Mathf.Cos(((float)i / amount) * Mathf.PI * 200), 0) * ((float)i / amount);
            data[i].pPos = data[i].pos;
			//data [i].pos = Random.insideUnitSphere;
            if (i>0)
                data[i].nPos = data[i - 1].pos;
        }

		buffer = new ComputeBuffer(data.Length, Vec.stride);
		buffer.SetData(data);
		//ComputeBuffer buffer2 = new ComputeBuffer(data.Length, Vec.stride);
		//buffer2.SetData(data);
		kernel = computeShader.FindKernel("Multiply");
		computeShader.SetBuffer(kernel, "dataBuffer", buffer);
		//computeShader.SetBuffer(kernel, "savedBuffer", buffer2);
		computeShader.Dispatch(kernel, data.Length, 1,1);

	}
    

	void RenderParticles()
	{
		Graphics.DrawMeshInstancedIndirect(_dummyMesh, 0, renderMaterial, _dummyMesh.bounds, _batchDrawArgs, 0, null, ShadowCastingMode.Off, true, gameObject.layer, Camera.main);
	}
    

	void UpdateShaderProperties(){
		computeShader.SetBuffer(kernel, "dataBuffer", buffer);
		computeShader.Dispatch(kernel, data.Length, 1,1);
		computeShader.SetVector ("Left", Left.transform.position);
		computeShader.SetVector ("Right", Right.transform.position);
        computeShader.SetInt ("amount", amount);
        computeShader.SetVector("NoiseSpeed", NoiseSpeed);
        computeShader.SetVector("NoiseAmount", NoiseAmount);
        computeShader.SetFloat("LeftSpeed", LeftSpeed);
        computeShader.SetFloat("RightSpeed", RightSpeed);
        computeShader.SetVector("NoiseFrequency", NoiseFrequency);
        LeftPrev = Left.transform.position;
		RightPrev = Right.transform.position;
	}

	void UpdateMaterialProperties()
	{
		renderMaterial.SetBuffer("Particles", buffer);
		renderMaterial.SetBuffer("meshData", _meshBuffer);
		renderMaterial.SetInt("MeshIndexCount", (int)ParticleMesh.triangles.Length);
		renderMaterial.SetFloat("_Scale", size);
        renderMaterial.SetColor("_Color", color);
        renderMaterial.SetPass(0);

	}

	////UTILS
	
	void CreateDrawArgsBuffer()
	{
		uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

		args[0] = (uint)amount * ParticleMesh.GetIndexCount(0);
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

	void CreateDummyMesh()
	{
		_dummyMesh = new Mesh();
		var totalVerts = amount * ParticleMesh.triangles.Length;
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

}
