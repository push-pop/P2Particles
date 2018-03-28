using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VectorFieldTexture {

    public bool hasLoaded = false;
    public TextAsset _vectorField;
    public TextureWrapMode _wrapMode;
    public bool _swapYZ;
    public RenderTexture Field;
    public VectorFieldLoader.VectorInfo[] _vectorInfo;
    public VectorFieldLoader.FieldInfo _info;

}
