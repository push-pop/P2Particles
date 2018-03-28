#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(SkinnedMeshData))]
public class SkinnerModelEditor : Editor
{
#region Editor functions

    public override void OnInspectorGUI()
    {
        var model = (SkinnedMeshData)target;
        EditorGUILayout.LabelField("Vertex Count", model.vertexCount.ToString());
    }

#endregion

#region Create menu item functions

    static Mesh[] SelectedMeshAssets
    {
        get
        {
            var assets = Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep);
            return assets.Select(x => (Mesh)x).ToArray();
        }
    }

    static bool CheckSkinned(Mesh mesh)
    {
        if (mesh.boneWeights.Length > 0) return true;
        Debug.LogError(
            "The given mesh (" + mesh.name + ") is not skinned. "
        );
        return false;
    }

    [MenuItem("Assets/Convert Mesh", true)]
    static bool ValidateAssets()
    {
        return SelectedMeshAssets.Length > 0;
    }

    [MenuItem("Assets/Convert Mesh")]
    static void ConvertAssets()
    {
        var converted = new List<Object>();

        foreach (var source in SelectedMeshAssets)
        {
            if (!CheckSkinned(source)) continue;

            // Destination file path.
            var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(source));
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/" + Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source)) +  "-points.asset");

            // Create a skinner model asset.
            var asset = ScriptableObject.CreateInstance<SkinnedMeshData>();
            asset.Init(source);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.AddObjectToAsset(asset.mesh, asset);

            converted.Add(asset);
        }

        // Save the generated assets.
        AssetDatabase.SaveAssets();

        // Select the generated assets.
        EditorUtility.FocusProjectWindow();
        Selection.objects = converted.ToArray();
    }

#endregion
}
#endif

