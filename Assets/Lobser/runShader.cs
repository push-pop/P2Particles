﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

public class runShader : MonoBehaviour {

	public int amount;
	public ComputeShader computeShader;
	int kernel = 0;

	public Texture spriteTexture;
	public float size;

	public Vector3 center;

//	public Material material;
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

	struct Vec
	{
		public Vector3 pos;
		public Vector3 vel;
		public const int stride = 6 * sizeof(float);
	}

	void Start(){
		
		CreateDummyMesh ();
		CreateDrawArgsBuffer();
		CreateSpriteMeshBuffer();
		//CreateBuffers();
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
			data [i].pos = Random.insideUnitSphere;
		}

		buffer = new ComputeBuffer(data.Length, Vec.stride);
		buffer.SetData(data);
		kernel = computeShader.FindKernel("Multiply");
		computeShader.SetBuffer(kernel, "dataBuffer", buffer);
		computeShader.Dispatch(kernel, data.Length, 1,1);

	}

	private void OnRenderObject()
	{
		RenderParticles();
	}

	void RenderParticles()
	{
		Graphics.DrawMeshInstancedIndirect(_dummyMesh, 0, renderMaterial, _dummyMesh.bounds, _batchDrawArgs, 0, null, ShadowCastingMode.Off, true, gameObject.layer, Camera.main);
	}

	void UpdateShaderProperties(){
		computeShader.SetBuffer(kernel, "dataBuffer", buffer);
		computeShader.Dispatch(kernel, data.Length, 1,1);
		computeShader.SetVector ("center", center);
	}

	void UpdateMaterialProperties()
	{
		renderMaterial.SetBuffer("Particles", buffer);
		renderMaterial.SetBuffer("meshData", _meshBuffer);
		renderMaterial.SetInt("MeshIndexCount", (int)ParticleMesh.triangles.Length);
		renderMaterial.SetFloat("_Scale", size);
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
