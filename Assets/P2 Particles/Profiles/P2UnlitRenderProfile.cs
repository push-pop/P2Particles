using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "P2UnlitRenderProfile", menuName = "P2Particles/Unlit Render Profile", order = 150)]
public class P2UnlitRenderProfile : ScriptableObject
{
    #region Particles2 properties
    [SerializeField, Range(0, 10000)]
    int _numParticles = 5000;

    [SerializeField]
    Particles2.EmitMode _emissionMode = Particles2.EmitMode.EmitFromSkinnedMesh;

    [Range(0, 10)]
    public float _emissionRadius = 1;

    [Range(0, 1)]
    public float _vectorFieldFollow;

    [SerializeField]
    public Vector3 _constantForce;

    [Range(-20, 20)]
    public float _noiseAmplitude;

    [Range(0, 10)]
    public float _noiseFrequency = 0.25f;

    [Range(0, 1)]
    public float _damping = 0.08f;

    [Range(0, 10)]
    public float _maxSpeed = 10;

    [Range(0, 1)]
    public float _maxSolverLife = 0.5f;

    #endregion


    #region P2Renderer Properties
    public P2Renderer.BlendMode _blendMode = P2Renderer.BlendMode.Alpha;

    public Vector4 _remapFBM = new Vector4(0, 1, 0, 1);
    public Vector4 _scaleByDist = new Vector4(.2f, 1f, 1f, 1);
    public float _scaleOnTime = 1f;
    [Range(0, 1)]
    public float _fbmAmt = 1f;
    [Range(0, 2)]
    public float _fbmFreq = 1f;


    [Range(0, 1)]
    public float _particlize = 1.0f;
    [Range(0.001f, .5f)]
    public float _particleSize = .07f;

    public Color _color = new Color(0, 1, 1, .2f);
    [Range(0, 1)]
    public float _falloff = 1;
    [Range(0, 1)]
    public float _hueSpeed = 0;
    [Range(0, 100)]
    public float _maxLife = 4;

    //[Range(0, 3)]
    //public float _uvFade = 1;

    public bool _debugVelocity = false;
    #endregion

    public void Load(P2Renderer r, Particles2 p)
    {

        //p._numParticles = _numParticles;
        p._emissionMode = _emissionMode;
        p._emissionRadius = _emissionRadius;
        p._vectorFieldFollow = _vectorFieldFollow;
        p._constantForce = _constantForce;
        p._noiseAmplitude = _noiseAmplitude;
        p._noiseFrequency = _noiseFrequency;
        p._damping = _damping;
        p._maxSpeed = _maxSpeed;
        p._maxLife = _maxSolverLife;

        r._blendMode = _blendMode;
        r._remapFBM = _remapFBM;
        r._scaleOnTime = _scaleOnTime;
        r._fbmAmt = _fbmAmt;
        r._fbmFreq = _fbmFreq;
        r._particlize = _particlize;
        r._particleSize = _particleSize;
        r._falloff = _falloff;
        r._hueSpeed = _hueSpeed;
        r._renderLife = _maxLife;
        //r._uvFade = _uvFade;
        r._debugVelocity = _debugVelocity;
    }

    public void Save(P2Renderer r, Particles2 p)
    {
        _numParticles = p._numParticles;
        _emissionMode = p._emissionMode;
        _emissionRadius = p._emissionRadius;
        _vectorFieldFollow = p._vectorFieldFollow;
        _constantForce = p._constantForce;
        _noiseAmplitude = p._noiseAmplitude;
        _noiseFrequency = p._noiseFrequency;
        _damping = p._damping;
        _maxSpeed = p._maxSpeed;
        _maxSolverLife = p._maxLife;

        _blendMode = r._blendMode;
        _remapFBM = r._remapFBM;
        _scaleOnTime = r._scaleOnTime;
        _fbmAmt = r._fbmAmt;
        _fbmFreq = r._fbmFreq;
        _particlize = r._particlize;
        _particleSize = r._particleSize;
        _falloff = r._falloff;
        _hueSpeed = r._hueSpeed;
        _maxLife = r._renderLife;
        //_uvFade = r._uvFade;
        _debugVelocity = r._debugVelocity;
    }

    public static void Lerp(P2UnlitRenderProfile p1, P2UnlitRenderProfile p2, float t, Particles2 p, P2Renderer r)
    {
        t = Mathf.Clamp(t, 0, 1);
        bool swap = t > 0.5f;

        p._numParticles = (int)Mathf.Lerp(p1._numParticles, p2._numParticles, t);
        p._emissionMode = swap ? p2._emissionMode : p1._emissionMode;
        p._emissionRadius = swap ? p2._emissionRadius : p1._emissionRadius;
        p._vectorFieldFollow = Mathf.Lerp(p1._vectorFieldFollow, p2._vectorFieldFollow, t);
        p._constantForce = Vector3.Lerp(p1._constantForce, p2._constantForce, t);
        p._noiseAmplitude = Mathf.Lerp(p1._noiseAmplitude, p2._noiseAmplitude, t);
        p._noiseFrequency = Mathf.Lerp(p1._noiseFrequency, p2._noiseFrequency, t);
        p._damping = Mathf.Lerp(p1._damping, p2._damping, t);
        p._maxSpeed = Mathf.Lerp(p1._maxSpeed, p2._maxSpeed, t);
        p._maxLife = Mathf.Lerp(p1._maxSolverLife, p2._maxSolverLife, t);


        r._blendMode = swap ? p2._blendMode : p1._blendMode;
        r._remapFBM = Vector4.Lerp(p1._remapFBM, p2._remapFBM, t);
        r._scaleOnTime = Mathf.Lerp(p1._scaleOnTime, p2._scaleOnTime, t);
        r._fbmAmt = Mathf.Lerp(p1._fbmAmt, p2._fbmAmt, t);
        r._fbmFreq = Mathf.Lerp(p1._fbmFreq, p2._fbmFreq, t);
        r._particlize = Mathf.Lerp(p1._particlize, p2._particlize, t);
        r._particleSize = Mathf.Lerp(p1._particleSize, p2._particleSize, t);
        r._falloff = Mathf.Lerp(p1._falloff, p2._falloff, t);
        r._hueSpeed = Mathf.Lerp(p1._hueSpeed, p2._hueSpeed, t);
        r._renderLife = Mathf.Lerp(p1._maxLife, p2._maxLife, t);
        //r._uvFade = Mathf.Lerp(p1._uvFade, p2._uvFade, t);
        r._debugVelocity = swap ? p2._debugVelocity : p1._debugVelocity;
    }

}
