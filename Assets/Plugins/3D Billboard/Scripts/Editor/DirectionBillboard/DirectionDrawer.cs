using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
namespace Com.DiazTeo.DirectionalSprite
{
    [Serializable]
    [CustomPropertyDrawer(typeof(Direction))]
    public class DirectionDrawer : PropertyDrawer
    {
        protected SerializedProperty spAngleStart;
        protected SerializedProperty spAngleEnd;
        protected SerializedProperty spInvert;
        protected SerializedProperty spSprite;
        protected float angleStart;
        protected float angleEnd;
        protected bool showPosition = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = "Direction";
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            showPosition = EditorGUILayout.Foldout(showPosition, label);
            EditorGUI.indentLevel = 1;

            if (showPosition)
            {

                spAngleStart = property.FindPropertyRelative("angleStart");
                spAngleEnd = property.FindPropertyRelative("angleEnd");
                spInvert = property.FindPropertyRelative("invertSprites");
                spSprite = property.FindPropertyRelative("sprites");
                angleStart = spAngleStart.floatValue;
                angleEnd = spAngleEnd.floatValue;

                EditorGUILayout.LabelField("Angles:");
                EditorGUILayout.BeginHorizontal();
                angleStart = EditorGUILayout.FloatField(angleStart);
                angleStart = Mathf.Clamp(angleStart, -180, angleEnd);
                EditorGUILayout.MinMaxSlider(ref angleStart, ref angleEnd, -180, 180);
                angleEnd = EditorGUILayout.FloatField(angleEnd);
                angleEnd = Mathf.Clamp(angleEnd, angleStart, 180);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.PropertyField(spInvert);
                spAngleStart.floatValue = angleStart;
                spAngleEnd.floatValue = angleEnd;
                EditorGUILayout.PropertyField(spSprite);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
