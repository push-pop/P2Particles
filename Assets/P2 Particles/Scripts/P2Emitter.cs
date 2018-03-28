using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Emitter : MonoBehaviour
{
    public struct LineSection
    {
        public Vector3 Start;
        public Vector3 End;

    }
    public int NumPoints
    {
        get
        {
            if (!isSetup)
                CollectPoints();
            return _emitterPoints.Count;
        }
    }

    public List<Vector3> Points
    {
        get
        {
            if (!isSetup)
                CollectPoints();
            return _emitterPoints;
        }
    }

    public bool connected = false;
    public float subdivs = 4f;
    public GameObject[] AnchorPoints;

    List<Vector3> _emitterPoints = new List<Vector3>();
    bool isSetup = false;

    // Use this for initialization
    void Awake()
    {
        CollectPoints();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePoints();
    }

    void UpdatePoints()
    {
        int index = 0;
        for (int i = 0; i < AnchorPoints.Length; i++)
        {
            var pt1 = AnchorPoints[i];
            if (i + 1 < AnchorPoints.Length || connected)
            {
                var idx = (i + 1) % AnchorPoints.Length;
                var pt2 = AnchorPoints[idx];

                foreach (var pt in GetPoints(pt1.transform.position, pt2.transform.position))
                {
                    _emitterPoints[index] =pt;
                    index++;
                }
            }
        }
    }

    void CollectPoints()
    {
        for (int i = 0; i < AnchorPoints.Length; i++)
        {
            var pt1 = AnchorPoints[i];
            if (i + 1 < AnchorPoints.Length || connected)
            {
                var idx = (i + 1) % AnchorPoints.Length;
                var pt2 = AnchorPoints[idx];

                foreach (var pt in GetPoints(pt1.transform.position, pt2.transform.position))
                {
                    _emitterPoints.Add(pt);
                }
            }
        }
        isSetup = true;
    }

    List<Vector3> GetPoints(Vector3 pt1, Vector3 pt2)
    {
        List<Vector3> pts = new List<Vector3>();
        Vector3 dir = (pt2 - pt1).normalized;
        float length = Vector3.Distance(pt1, pt2);
        float step = length / subdivs;
        float offset = step / 2f;

        for (int i = 0; i < subdivs; i++)
        {
            Vector3 pt = pt1 + dir * (offset + step * i);
            pts.Add(pt);
        }


        return pts;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < AnchorPoints.Length; i++)
        {
            var pt1 = AnchorPoints[i];
            if (i + 1 < AnchorPoints.Length || connected)
            {
                var idx = (i + 1) % AnchorPoints.Length;
                var pt2 = AnchorPoints[idx];

                //foreach (var pt in GetPoints(pt1.transform.position, pt2.transform.position))
                //{
                //   // Gizmos.DrawWireSphere(pt, 0.03f);
                //}

                Gizmos.DrawLine(pt1.transform.position, pt2.transform.position);


            }


        }
    }
}
