using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveSin : MonoBehaviour
{

    public Vector3 _speed = new Vector3(1, 1, 1);
    public Vector3 _offset;
    public Vector3 _amplitude = new Vector3(1, 1, 1);
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_amplitude.x * Mathf.Sin(Mathf.Deg2Rad * (_speed.x * Time.time + _offset.x)), _amplitude.y * Mathf.Sin(Mathf.Deg2Rad * (_speed.y * Time.time + _offset.y)), _amplitude.z * Mathf.Sin(Mathf.Deg2Rad * (_speed.z * Time.time + _offset.z)));
    }
}
