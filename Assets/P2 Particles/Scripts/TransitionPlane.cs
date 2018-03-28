using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransitionPlane : MonoBehaviour
{
	[SerializeField]
	private Gradient _gradient;

	private Texture2D _gradientTex;
	public Texture2D GradientTexture
	{
		get
		{
			return _gradientTex;
		}
	}

	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float _fadeDistance;
	public float FadeDistance
	{
		get
		{
			return _fadeDistance;
		}
	}

	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float _noiseInfluence;
	public float NoiseInfluence
	{
		get
		{
			return _noiseInfluence;
		}
	}

	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float _colorIntensity = 1.0f;
	public float ColorIntensity
	{
		get
		{
			return _colorIntensity;
		}
	}

	[SerializeField]
	private bool _cullParticles = true;
	public float CullParticles
	{
		get
		{
			return _cullParticles == true ? 1.0f : 0.0f;
		}
	}

	public Vector3 Position
	{
		get { return transform.position; }
	}

	public Vector3 Normal
	{
		get { return transform.forward; }
	}

	private void Start()
	{
		_gradientTex = new Texture2D(256, 1, TextureFormat.RGBA32, false);
		_gradientTex.filterMode = FilterMode.Point;
		_gradientTex.wrapMode = TextureWrapMode.Clamp;

		UpdateTexture();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			UpdateTexture();
		}
	}

	private void UpdateTexture()
	{
		Debug.Log("Updating gradient texture");
		// sample & copy gradient to texture
		float step = 1.0f / (float)_gradientTex.width;
		float t = 0;

		for (int x = 0; x <= _gradientTex.width; ++x)
		{
			_gradientTex.SetPixel(x, 0, _gradient.Evaluate(t));
			t += step;
		}

		_gradientTex.Apply(); // upload texture changes
	}
}
