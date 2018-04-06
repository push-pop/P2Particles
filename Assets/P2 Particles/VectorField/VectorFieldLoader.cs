using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class VectorFieldLoader : Singleton<VectorFieldLoader>
{
    private Dictionary<string, VectorFieldTexture> _vFieldDictionary = new Dictionary<string, VectorFieldTexture>();

    #region Compute

    ComputeBuffer _vectorBuffer;
    ComputeBuffer _vFieldInfo;

    [SerializeField]
    ComputeShader _bakeTextureShader;

    [System.Serializable]
    public struct VectorInfo
    {
        public Vector3 Position;
        public Vector3 Direction;

        public static int stride = 6 * sizeof(float);
    }

    [System.Serializable]
    public struct FieldInfo
    {
        public Vector3 Resolution;
        public Vector3 BoundingMinimum;
        public Vector3 BoundingMaximum;

        public static int stride = 9 * sizeof(float);
    }

    #endregion

    #region Loading Vectorfield Methods

    Vector3 to3D(int idx, FieldInfo info)
    {
        return new Vector3(idx % info.Resolution.x,
            (idx / info.Resolution.x) % info.Resolution.y,
            idx / (info.Resolution.x * info.Resolution.y));
    }

    private void BakeVectorField(ref VectorFieldTexture v)
    {

        ParseVectorField(ref v);

        CreateBuffers(v);

        DoBakeTexture(v);

        DestroyBuffers();

        v.hasLoaded = true;

        Debug.Log("Loaded VectorField: " + v._vectorField.name);
    }

    public VectorFieldTexture Load(FieldToLoad fieldAsset)
    {
        var field = GetField(fieldAsset.textAssset);

        if (field == null)
        {
            Debug.Log("Something went really wrong loading vField");
            return null;
        }

        if (!field.hasLoaded)
        {
            field._vectorField = fieldAsset.textAssset;
            field._swapYZ = fieldAsset.swapYZ;
            field._wrapMode = fieldAsset.wrapMode;

            BakeVectorField(ref field);
        }

        return field;
    }

    void DoBakeTexture(VectorFieldTexture v)
    {
        var bake = _bakeTextureShader;
        Vector3 res = v._info.Resolution;
        var data = new Color[(int)(res.x * res.y * res.z)];


        var t = new RenderTexture((int)res.x, (int)res.y, (int)res.z, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
        {
            name = v._vectorField.name,
            filterMode = FilterMode.Trilinear,
            wrapMode = v._wrapMode,
            volumeDepth = (int)res.z,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true,

        };

        if (!t.Create())
            Debug.Log("Failed Create RenderTexture");

        var kernel = bake.FindKernel("CSMain");
        bake.SetBool("_swapYZ", v._swapYZ);
        bake.SetBuffer(kernel, "_vFieldInfo", _vFieldInfo);
        bake.SetTexture(kernel, "_result", t);
        bake.SetBuffer(kernel, "_vectorField", _vectorBuffer);

        bake.Dispatch(kernel, (int)(res.x * res.y * res.z), 1, 1);

        v.Field = t;

    }

    void CreateBuffers(VectorFieldTexture v)
    {

        if (_vectorBuffer != null)
            _vectorBuffer.Release();

        _vectorBuffer = new ComputeBuffer(v._vectorInfo.Length, VectorInfo.stride);

        _vectorBuffer.SetData(v._vectorInfo);

        _vFieldInfo = new ComputeBuffer(1, FieldInfo.stride);
        var info = new FieldInfo[]
            {
                new FieldInfo
                {
                    Resolution = v._info.Resolution,
                    BoundingMaximum = v._info.BoundingMaximum,
                    BoundingMinimum = v._info.BoundingMinimum
                }
            };

        _vFieldInfo.SetData(info);
    }

    void DestroyBuffers()
    {
        if (_vectorBuffer != null)
            _vectorBuffer.Release();
    }

    void ParseVectorField(ref VectorFieldTexture v)
    {
        var grid = SplitCsvGrid(v._vectorField.text);

        v._info = new FieldInfo();
        v._info.Resolution = new Vector3(float.Parse(grid[0, 0]), float.Parse(grid[1, 0]), float.Parse(grid[2, 0]));
        v._info.BoundingMinimum = new Vector3(float.Parse(grid[0, 1]), float.Parse(grid[1, 1]), float.Parse(grid[2, 1]));
        v._info.BoundingMaximum = new Vector3(float.Parse(grid[0, 2]), float.Parse(grid[1, 2]), float.Parse(grid[2, 2]));

        ParseVectors(grid, ref v);

    }

    void ParseVectors(string[,] grid, ref VectorFieldTexture v)
    {
        var numVecs = grid.GetUpperBound(1);
        v._vectorInfo = new VectorInfo[numVecs - 3];

        var info = v._info;

        var step = Vector3.Scale((info.BoundingMaximum - info.BoundingMinimum), new Vector3(1f / info.Resolution.x, 1f / info.Resolution.y, 1f / info.Resolution.z));

        for (int i = 3; i < numVecs; i++)
        {
            int index = i - 3;
            var vec = new Vector3(float.Parse(grid[0, i]), float.Parse(grid[1, i]), float.Parse(grid[2, i]));

            var offset = 0.5f * step;
            var pos = info.BoundingMinimum + Vector3.Scale(step, to3D(index, info)) + offset;

            v._vectorInfo[index].Direction = new Vector3(vec.x, vec.y, vec.z);
            v._vectorInfo[index].Position = pos;
        }
    }

    public VectorFieldTexture GetField(TextAsset t)
    {
        VectorFieldTexture v = null;
        var name = t.name.ToLower();
        if (!_vFieldDictionary.TryGetValue(name, out v))
        {
            Debug.Log("Couldn't find vectorfieldTexture... creating new one");
            v = new VectorFieldTexture();
            _vFieldDictionary.Add(t.name.ToLower(), v);
        }
        else
        {
            Debug.Log("Found Existing VectorField Texture");
        }

        return v;
    }

    #endregion

    #region CSV Parsing
    static public void DebugOutputGrid(string[,] grid)
    {
        string textOutput = "";
        for (int y = 0; y < grid.GetUpperBound(1); y++)
        {
            for (int x = 0; x < grid.GetUpperBound(0); x++)
            {

                textOutput += grid[x, y];
                textOutput += "|";
            }
            textOutput += "\n";
        }
        Debug.Log(textOutput);
    }

    // splits a CSV file into a 2D string array
    static private string[,] SplitCsvGrid(string csvText)
    {
        string[] lines = csvText.Split("\n"[0]);

        // finds the max width of row
        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        // creates new 2D string grid to output to
        string[,] outputGrid = new string[width + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = SplitCsvLine(lines[y]);
            for (int x = 0; x < row.Length; x++)
            {
                outputGrid[x, y] = row[x];

                // This line was to replace "" with " in my output. 
                // Include or edit it as you wish.
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    // splits a CSV row 
    static public string[] SplitCsvLine(string line)
    {
        return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
        @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }
    #endregion

    private void Awake()
    {
        if (_bakeTextureShader == null)
            _bakeTextureShader = Resources.Load<ComputeShader>("BakeVectorField");

    }
}
