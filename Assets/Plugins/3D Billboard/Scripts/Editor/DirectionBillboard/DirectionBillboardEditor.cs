using DiazTeo.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Com.DiazTeo.DirectionalSprite;
using UnityEditor.Animations;

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
        protected AnimatorController animator;

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


            if (GUI.Button(EditorGUILayout.GetControlRect(), "Convert To Animation"))
            {
                Export();
            }

            //Animator
            EditorGUI.BeginChangeCheck();
            AnimatorController newAnimator = EditorGUILayout.ObjectField("Current Animator", animator, typeof(AnimatorController), false) as AnimatorController;
            if (EditorGUI.EndChangeCheck())
            {
                RegisterUndo("Change Current Animator");
                animator = newAnimator;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void Export()
        {
            List<AnimationClip> clips = new List<AnimationClip>();

            foreach (Direction direction in billboard.directions)
            {
                AnimationClip clip = new AnimationClip();
                clip.name = billboard.name + "_" + direction.angleStart + "_" + direction.angleEnd;
                clip.wrapMode = billboard.loop ? WrapMode.Loop : WrapMode.Default;
                AnimationClipSettings settings = new AnimationClipSettings();
                settings.loopTime = billboard.loop;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                clip.ClearCurves();
                if (direction.sprites.Count != 0)
                {
                    //Set Keyframes
                    EditorCurveBinding curveBinding = new EditorCurveBinding();
                    curveBinding.type = typeof(SpriteRenderer);
                    curveBinding.path = "";
                    curveBinding.propertyName = "m_Sprite";
                    ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[direction.sprites.Count];
                    for (int i = 0; i < direction.sprites.Count; i++)
                    {
                        Sprite sprite = direction.sprites[i];
                        keyFrames[i] = new ObjectReferenceKeyframe();
                        keyFrames[i].time = i * billboard.waitTime;
                        keyFrames[i].value = sprite;

                    }
                    AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

                    //Set FlipX
                    AnimationCurve curve = direction.invertSprites
                        ? AnimationCurve.Constant(0, (direction.sprites.Count) * billboard.waitTime, 1)
                        : AnimationCurve.Constant(0, direction.sprites.Count * billboard.waitTime, 0);

                    clip.SetCurve("", typeof(SpriteRenderer), "m_FlipX", curve);

                }
                clips.Add(clip);
            }

            string path = EditorUtility.SaveFilePanelInProject("Export Animations", "animations", "", "Export Animations");
            if (path == "")
                return;

            foreach (AnimationClip clip in clips)
            {
                string clipPath = path + clip.name + ".anim";
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            AssetDatabase.SaveAssets();

            AnimatorController controller = null;
            AnimatorStateMachine rootStateMachine = null;

            switch (animator)
            {
                case null:
                    controller = AnimatorController.CreateAnimatorControllerAtPath(path + ".controller");
                    controller.AddParameter("angle", AnimatorControllerParameterType.Float);
                    break;
                default:
                    controller = animator;
                    break;
            }

            rootStateMachine = controller.layers[0].stateMachine;
            AnimatorStateMachine stateMachine = rootStateMachine.AddStateMachine(billboard.name, Vector3.zero);
            AnimatorState baseState = stateMachine.AddState("Base", new Vector3(0, 600));

            for (int x = 0; x < billboard.directions.Count; x++)
            {
                Direction direction = billboard.directions[x];
                string name = billboard.name + "_" + direction.angleStart + "_" + direction.angleEnd;
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path + name + ".anim");

                float radians = (((float)x / (billboard.directions.Count)) * 360f) * Mathf.Deg2Rad;
                var cos = Mathf.Cos(radians);
                var sin = Mathf.Sin(radians);
                Vector3 pos = new Vector3(0, 600);
                pos += new Vector3(cos, sin) * 300;

                AnimatorState state = stateMachine.AddState(name, pos);
                state.motion = clip;
                var transition = AddTransition(state, baseState);
                transition.AddCondition(AnimatorConditionMode.Greater, direction.angleEnd, "angle");

                transition = AddTransition(state, baseState);
                transition.AddCondition(AnimatorConditionMode.Less, direction.angleStart, "angle");


                transition = AddTransition(baseState, state);
                transition.AddCondition(AnimatorConditionMode.Greater, direction.angleStart, "angle");
                transition.AddCondition(AnimatorConditionMode.Less, direction.angleEnd, "angle");
            }
            AssetDatabase.SaveAssets();
        }

        protected AnimatorStateTransition AddTransition(AnimatorState baseState, AnimatorState endState)
        {
            var transition = baseState.AddTransition(endState);
            transition.duration = 0;
            transition.hasExitTime = false;
            return transition;
        }

        protected void ShowDirection()
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

        protected void ChangeVariable()
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

        protected void ShowSprites(float angle)
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

        protected void DoSpriteView()
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

                if (!EditorGUIUtility.isProSkin)
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

        protected void DoSpriteViewForeground()
        {
            Color backgroundColor = Contents.proBackgroundColor;
            if (!EditorGUIUtility.isProSkin)
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
            Vector3 dir = directionSettings == 0 ? rot * Vector3.down : rot * Vector3.up;
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

            switch (directionSettings)
            {
                case 0:
                    newAngle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
                    break;
                default:
                    newAngle = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);
                    newAngle *= -1;
                    break;
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
                Vector3 dir = directionSettings == 0
                    ? Quaternion.AngleAxis(angleBegin, Vector3.forward) * Vector3.down
                    : Quaternion.AngleAxis(angleBegin, Vector3.forward) * Vector3.up;
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
                    billboard.directions[i].AngleEnd = billboard.GetNewAngleEnd(billboard.directions[i], newAngleEnd);
                    serializedObject.ApplyModifiedProperties();
                }
            }

        }

        private float CreateHandler(float angle)
        {
            int directionSettings = settings.FindProperty("currentDirection").enumValueIndex;

            float ang;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = directionSettings == 0 ? rot * Vector3.down : rot * Vector3.up;

            dir.Normalize();
            Vector3 pos = (Vector3)m_SpriteViewRect.center + dir * Contents.handleRadius;
            float distance = Vector3.Distance(pos, m_SpriteViewRect.center);

            if (distance > Contents.handleRadius || distance < Contents.handleRadius)
            {
                Vector3 fromOriginToObject = pos - (Vector3)m_SpriteViewRect.center;
                fromOriginToObject *= Contents.handleRadius / distance;
                pos = (Vector3)m_SpriteViewRect.center + fromOriginToObject;
            }

            pos = Handles.Slider2D(pos, Vector3.forward, Vector3.up, Vector3.right, 10, Handles.DotHandleCap, 1f);

            Vector3 direction = (Vector3)m_SpriteViewRect.center - pos;
            direction.Normalize();
            pos -= (Vector3)m_SpriteViewRect.center;

            switch (directionSettings)
            {
                case 0:
                    ang = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
                    break;
                default:
                    ang = Vector3.SignedAngle(Vector3.down, direction, Vector3.forward);
                    ang *= -1;
                    break;
            }

            return ang;
        }
    }
}
