using UnityEditor;
using UnityEngine;

namespace DiazTeo.Editor.Utils
{
    public static class EditorGUILayoutUtility
    {
        public static void DrawHeader(GUIContent content, GUIStyle style)
        {
            EditorGUILayout.LabelField(content, style);
        }

        public static void DrawHeader(GUIContent content)
        {
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
        }

        public static void DrawHeader(string content)
        {
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
        }

        public static void DrawHeader(string content, GUIStyle style)
        {
            EditorGUILayout.LabelField(content, style);
        }
    }
}
