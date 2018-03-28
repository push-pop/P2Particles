//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(P2Renderer))]
////[CanEditMultipleObjects]
//public class P2RendererEditor : Editor {

//    SerializedProperty _shader;

//	// Use this for initialization
//	void OnEnable () {
//        _shader = serializedObject.FindProperty("_shader");


//	}

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        EditorGUILayout.PropertyField(_shader);

//        var s = _shader.objectReferenceValue as Shader;
//        if (s == null)
//        {
//            Debug.Log("Shader Null");
//            s = Shader.Find("P2/UnlitParticles");
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
