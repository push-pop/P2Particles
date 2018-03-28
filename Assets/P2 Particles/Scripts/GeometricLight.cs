using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometricLight : MonoBehaviour
{
    [SerializeField, Range(0, 2)]
    private float _focusRadius = 1.0f;
    public float FocusRadius
    {
        get { return _focusRadius; }
    }

    [SerializeField, Range(0, 5)]
    private float _gradientRadius = 2.0f;
    public float GradientRadius
    {
        get { return _gradientRadius; }
    }

    [SerializeField, Range(1, 3)]
    private float _falloffExponent = 2;
    public float FalloffExponent
    {
        get { return _falloffExponent; }
    }

    public Vector3 Position
    {
        get { return transform.position; }
    }

    private void Start()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Position, FocusRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Position, GradientRadius);
    }
}
