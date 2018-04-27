using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FieldInfluencerData
{
    public Vector3 position;
    public Vector3 velocity;
    public float force;

    public const int stride = 7 * sizeof(float);
}

[ExecuteInEditMode]
public class FieldInfluencer : MonoBehaviour
{
    public FieldInfluencerData Data { get { return _data; } }
    public float _MaxInfluence = 1f;
    [SerializeField]
    FieldInfluencerData _data;

    [SerializeField]
    float _clampMagnitude = 100f;

    [SerializeField, Range(0, 10)]
    float _force = 1;

    Vector3 _lastPosition;
    // Use this for initialization
    void Start()
    {
        _lastPosition = transform.position;
        _data = new FieldInfluencerData()
        {
            position = transform.position,
            velocity = Vector3.zero,
            force = _force
        };
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = _data.position;
        _data.position = transform.position;
        _data.velocity = Vector3.ClampMagnitude(_lastPosition - _data.position, _clampMagnitude);
        _data.force = _force;
    }
}
