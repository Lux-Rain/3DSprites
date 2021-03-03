using DiazTeo.Editor.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Com.DiazTeo.DirectionalSprite;

namespace Com.DiazTeo.DirectionalSpriteEditor
{

    [CustomEditor(typeof(DirectionalBillboard), true)]
    public class DirectionBillboardEditor : Editor
    {
        private static class Contents
        {
            public static readonly float spritePreviewRectSize = 350;
            public static readonly float spritePreviewRadius = 150;
            public static readonly float handleRadius = 140;

            public static readonly GUIContent nameLabel = new GUIContent("Directional Sprite");
            public static readonly GUIContent spriteViewLabel = new GUIContent("Sprite View");
            public static readonly GUIContent spritesLabel = new GUIContent("Sprites");
            public static readonly GUIContent angleRangesLabel = new GUIContent("Angle Ranges");
            public static readonly GUIContent angleRangeLabel = new GUIContent("Angle Range ({0})");

            public static readonly Color proBackgroundColor = new Color32(49, 77, 121, 255);
            public static readonly Color proBackgroundRangeColor = new Color32(25, 25, 25, 128);
            public static readonly Color proColor1 = new Color32(10, 46, 42, 255);
            public static readonly Color proColor2 = new Color32(33, 151, 138, 255);
            public static readonly Color defaultColor1 = new Color32(25, 61, 57, 255);
            public static readonly Color defaultColor2 = new Color32(47, 166, 153, 255);
            public static readonly Color defaultBackgroundColor = new Color32(64, 92, 136, 255);
            public static readonly Color backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1);
        }

        protected Direction m_currentDirection;
        private Sprite m_PreviewSprite;

        protected Rect m_SpriteViewRect;
        protected DirectionalBillboard billboard;
        protected SerializedProperty spDirectionList;
        protected SerializedProperty spWaitTime;
        protected SerializedProperty spLoop;
        protected SerializedProperty spReset;

        protected float currentAngle;
        protected bool showAllDirection;
        protected float t;
        protected int currentFrame = 0;

        protected SerializedObject settings;
        public void RegisterUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(billboard, name);
            Undo.RegisterCompleteObjectUndo(this, name);
            EditorUtility.SetDirty(billboard);
        }

        private void OnEnable()
        {
            billboard = (DirectionalBillboard)target;
            settings = DirectionBillboardSettings.GetSerializedSettings();
            spDirectionList = serializedObject.FindProperty("directions");
            spWaitTime = serializedObject.FindProperty("waitTime");
            spLoop = serializedObject.FindProperty("loop");
            spReset = serializedObject.FindProperty("resetFrameWhenChangeDirection");
            EditorApplication.update = Update;
        }

        private void OnDisable()
        {
            EditorApplication.update = null;
        }

        public void Update()
        {
            t += Time.deltaTime;
            EditorUtility.SetDirty(target);
            settings.Update();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //Name
            EditorGUILayoutUtility.DrawHeader(Contents.nameLabel);
            EditorGUILayout.Space();

            //SpriteView
            EditorGUILayoutUtility.DrawHeader(Contents.spriteViewLabel);
            DoSpriteView();

            if (targets.Length == 1)
            {
                DoDirectionRangeGUI();
            }
            DoSpriteViewForeground();
            if (targets.Length == 1)
            {
                DoDirectionRange();
                DoRange();
            }
            ChangeVariable();
            EditorGUILayout.Space();
            //Sprites
            EditorGUILayoutUtility.DrawHeader(Contents.spritesLabel);
            ShowDirection();
            ShowSprites(currentAngle);
            serializedObject.ApplyModifiedProperties();
        }



        private void ShowDirection()
        {
            showAllDirection = EditorGUILayout.Toggle("Show All Direction", showAllDirection);
            if (!showAllDirection)
            {
                DoSpriteList();
            }
            else
            {
                EditorGUILayout.PropertyField(spDirectionList);
            }
        }

        private void ChangeVariable()
        {
            EditorGUI.BeginChangeCheck();
            float waitTime = EditorGUILayout.FloatField("Wait Time", spWaitTime.floatValue);
            bool loop = EditorGUILayout.Toggle("Loop", spLoop.boolValue);
            bool reset = EditorGUILayout.Toggle("Reset Animation When Change Direction", spReset.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                RegisterUndo("Register Variable");
                spWaitTime.floatValue = waitTime;
                spLoop.boolValue = loop;
                spReset.boolValue = reset;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void ShowSprites(float angle)
        {
            int index = 0;
            if (CheckDirectionList(angle, out index))
            {
                if (index < billboard.directions.Count)
                {
                    ShowSprite(billboard.directions[index].sprites, billboard.directions[index].invertSprites);
                }
            }
        }

        protected void ShowSprite(List<Sprite> sprites, bool flip)
        {
            Sprite sprite;
            Vector2 pos = new Vector2(m_SpriteViewRect.center.x - Contents.spritePreviewRadius / 2, m_SpriteViewRect.center.y - Contents.spritePreviewRadius / 2);
            Vector2 size = new Vector2(Contents.spritePreviewRadius, Contents.spritePreviewRadius);
            if (sprites.Count != 0)
            {
                if (t >= spWaitTime.floatValue)
                {
                    t = 0;
                    currentFrame++;
                    if (currentFrame > sprites.Count - 1)
                    {
                        currentFrame = 0;
                    }
                }
                sprite = sprites[currentFrame];
                if (sprite == null)
                {
                    return;
                }
                if (m_PreviewSprite != sprite)
                {
                    m_PreviewSprite = sprite;
                }
                var material = EditorSpriteGUIUtility.spriteMaterial;
                Texture2D texture = EditorSpriteGUIUtility.GetTextureFromSingleSprite(sprite);
                if (flip)
                {
                    texture = EditorSpriteGUIUtility.FlipTexture(texture);
                }
                EditorGUI.DrawPreviewTexture(new Rect(pos, size), texture, material, ScaleMode.ScaleToFit);
            }
        }

        protected void DoSpriteList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            int index = 0;
            if (GUILayout.Button("Create Direction"))
            {
                RegisterUndo("Create Direction");
                Direction direction = new Direction();
                direction.AngleStart = currentAngle - 20; 
                direction.AngleEnd = currentAngle + 20;
                serializedObject.ApplyModifiedProperties();
                direction.AngleStart = billboard.GetNewAngleStart(direction, direction.angleStart);
                direction.AngleEnd = billboard.GetNewAngleEnd(direction, direction.angleEnd);
                billboard.directions.Add(direction);
                serializedObject.ApplyModifiedProperties();

            }
            if (GUILayout.Button("Remove Direction"))
            {
                if (CheckDirectionList(currentAngle, out index))
                {
                    RegisterUndo("Remove Direction");
                    billboard.directions.RemoveAt(index);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (CheckDirectionList(currentAngle, out index))
            {
                EditorGUILayout.PropertyField(spDirectionList.GetArrayElementAtIndex(index));
            }
        }

        protected bool CheckDirectionList(float angle, out int index)
        {
            Direction direction;
            for (int i = billboard.directions.Count - 1; i >= 0; i--)
            {
                direction = billboard.directions[i];
                if (direction.AngleStart <= angle && direction.AngleEnd >= angle)
                {
                    index = i;
                    return true;
                }

            }
            index = 0;
            return false;
        }

        public void DoSpriteView()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            Rect rect = EditorGUILayout.GetControlRect(false, Contents.spritePreviewRectSize);
            if (Event.current.type == EventType.Repaint)
                m_SpriteViewRect = rect;
            {

                //Draw background
                Color backgroundRangeColor = Contents.proBackgroundRangeColor;

                if (!UnityEditor.EditorGUIUtility.isProSkin)
                {
                    backgroundRangeColor.a = 0.1f;
                }

                Color c = Handles.color;
                Handles.color = backgroundRangeColor;
                Handles.DrawSolidDisc(rect.center, Vector3.forward, Contents.spritePreviewRadius);
                Handles.color = c;

            }
            float newAngle = EditorGUILayout.Slider("Current Angle", currentAngle, -180, 180);
            if (EditorGUI.EndChangeCheck())
            {
                currentAngle = newAngle;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void DoSpriteViewForeground()
        {
            Color backgroundColor = Contents.proBackgroundColor;
            if (!UnityEditor.EditorGUIUtility.isProSkin)
            {
                backgroundColor = Contents.defaultBackgroundColor;
            }

            Color c = Handles.color;
            Handles.color = backgroundColor;
            Handles.DrawSolidDisc(m_SpriteViewRect.center, Vector3.forward, Contents.spritePreviewRadius * 0.9f);
            Handles.color = c;
        }

        protected void DoRange()
        {
            int directionSettings = settings.FindProperty("currentDirection").enumValueIndex;
            Handles.color = Color.white;

            EditorGUI.BeginChangeCheck();
            
            Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.forward);
            Vector3 dir;
            if (directionSettings == 0)
            {
                dir = rot * Vector3.down;
            }
            else
            {
                dir = rot * Vector3.up;
            }
            dir.Normalize();
            Vector3 pos = (Vector3)m_SpriteViewRect.center + dir * Contents.handleRadius;
            pos = Handles.Slider2D(pos, Vector3.forward, Vector3.up, Vector3.right, 10, Handles.DotHandleCap, 1f);

            float distance = Vector3.Distance(pos, m_SpriteViewRect.center);
            if (distance > Contents.handleRadius || distance < Contents.handleRadius) //If the distance is less than the radius, it is already within the circle.
            {
                Vector3 fromOriginToObject = pos - (Vector3)m_SpriteViewRect.center; //~GreenPosition~ - *BlackCenter*
                fromOriginToObject *= Contents.handleRadius / distance; //Multiply by radius //Divide by Distance
                pos = (Vector3)m_SpriteViewRect.center + fromOriginToObject; //*BlackCenter* + all that Math
            }

            Vector3 direction = (Vector3)m_SpriteViewRect.center - pos;
            direction.Normalize();
            pos -= (Vector3)m_SpriteViewRect.center;

            float newAngle;
            if (directionSettings == 0)
            {
                newAngle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            }
            else
            {
                newAngle = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);
                newAngle *= -1;
            }

            if (EditorGUI.EndChangeCheck())
            {
                currentAngle = newAngle;
                serializedObject.ApplyModifiedProperties();
            }


        }

        protected void DoDirectionRangeGUI()
        {
            int directionSettings = settings.FindProperty("currentDirection").enumValueIndex;

            for (int i = billboard.directions.Count - 1; i >= 0; i--)
            {

                Color color = Color.Lerp(Contents.proColor1, Contents.proColor2, (float)i / (billboard.directions.Count - 1));
                Handles.color = color;
                float angleBegin = billboard.directions[i].AngleStart;
                float angleEnd = billboard.directions[i].AngleEnd;
                float angle = angleEnd - angleBegin;
                Vector3 dir;
                if (directionSettings == 0)
                {
                    dir = (Quaternion.AngleAxis(angleBegin, Vector3.forward) * Vector3.down);
                } else
                {
                    dir = (Quaternion.AngleAxis(angleBegin, Vector3.forward) * Vector3.up);
                }
                Handles.DrawSolidArc(m_SpriteViewRect.center, Vector3.forward, dir, angle, Contents.spritePreviewRadius);
            }
        }

        protected void DoDirectionRange()
        {
            EditorGUI.BeginChangeCheck();
            for (int i = billboard.directions.Count - 1; i >= 0; i--)
            {
                Color color = Color.Lerp(Contents.proColor1, Contents.proColor2, (float)i / (billboard.directions.Count - 1));
                Handles.color = color;
                float angleBegin = billboard.directions[i].AngleStart;
                float angleEnd = billboard.directions[i].AngleEnd;
                EditorGUI.BeginChangeCheck();
                float newAngleBegin = CreateHandler(angleBegin);
                float newAngleEnd = CreateHandler(angleEnd);
                if (EditorGUI.EndChangeCheck())
                {
                    RegisterUndo("Change Angle");
                    billboard.directions[i].AngleStart = billboard.GetNewAngleStart(billboard.directions[i], newAngleBegin);
                    billboard.directions[i].AngleEnd = billboard.GetNewAngleEnd(billboard.directions[i],newAngleEnd);
                    serializedObject.ApplyModifiedProperties();

                }
            }
        }

        private float CreateHandler(float angle)
        {
            Texture icon = Resources.Load("handleIcon") as Texture;
            int directionSettings = settings.FindProperty("currentDirection").enumValueIndex;

            float ang;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir;
            if (directionSettings == 0)
            {
                dir = rot * Vector3.down;
            }
            else
            {
                dir = rot * Vector3.up;

            }
            dir.Normalize();
            Vector3 pos = (Vector3)m_SpriteViewRect.center + dir * Contents.handleRadius;
            float distance = Vector3.Distance(pos, m_SpriteViewRect.center);
            if (distance > Contents.handleRadius || distance < Contents.handleRadius) //If the distance is less than the radius, it is already within the circle.
            {
                Vector3 fromOriginToObject = pos - (Vector3)m_SpriteViewRect.center; //~GreenPosition~ - *BlackCenter*
                fromOriginToObject *= Contents.handleRadius / distance; //Multiply by radius //Divide by Distance
                pos = (Vector3)m_SpriteViewRect.center + fromOriginToObject; //*BlackCenter* + all that Math
            }
            pos = Handles.Slider2D(pos, Vector3.forward, Vector3.up, Vector3.right, 10, Handles.DotHandleCap, 1f);

            Vector3 direction = (Vector3)m_SpriteViewRect.center - pos;
            direction.Normalize();
            pos -= (Vector3)m_SpriteViewRect.center;

            if (directionSettings == 0)
            {
                ang = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            }
            else
            {
                ang = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);
                ang *= -1;
            }
            return ang;
        }
    }
}
