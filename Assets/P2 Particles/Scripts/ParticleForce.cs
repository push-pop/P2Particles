using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleForce : MonoBehaviour
{

    #region ComputeBuffer Structures
    [System.Serializable]
    public struct ForceInfo
    {
        public Vector3 Center;
        public float Range;
        public float Falloff;
        public float Power;

        public static int stride = 6 * sizeof(float);
    }
    #endregion 

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _forceInfo.Center = transform.position;
    }

    public ForceInfo Info
    {
        get { return _forceInfo; }
    }

    [SerializeField]
    ForceInfo _forceInfo;
}
