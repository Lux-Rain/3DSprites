using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Com.DiazTeo.DirectionalSprite;
using System;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.Linq;

namespace Com.DiazTeo.DirectionalSpriteEditor
{   
    public class DirectionBillboardGraph : EditorWindow {

        public static DirectionBillboardGraph window;
        protected Rect nodePosition = new Rect(100, 200, 100, 150);
        protected DirectionBillboardGraphView _graphView;

        protected string fileName = "New Animator Converter";
        protected Box dragAndDropBox;
        [MenuItem("3DSprites/Convert To Animator")]
        public static void Init() {
            window = GetWindow<DirectionBillboardGraph>();
            window.titleContent = new GUIContent("Convert To Animator");
            window.Show();
        }

        public void Construct()
        {
            window = GetWindow<DirectionBillboardGraph>();
            ConstructGraph();
            GenerateToolBar();
            GenerateMiniMap();
        }

        private void OnEnable()
        {
            Construct();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        public void ConstructGraph()
        {
            dragAndDropBox = new Box();
            dragAndDropBox.StretchToParentSize();
            rootVisualElement.Add(dragAndDropBox);
            _graphView = new DirectionBillboardGraphView
            {
                name = "Convert To Animator"
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);


        }

        public void GenerateToolBar()
        {
            var toolbar = new Toolbar();
            var nodeCreateButton = new Button(() =>
            {
                _graphView.CreateNode(null);
            });
            nodeCreateButton.text = "CreateNode";

            var exportToAnimator = new Button(() =>
            {
                Export();
            });
            exportToAnimator.text = "export";

            toolbar.Add(nodeCreateButton);
            toolbar.Add(exportToAnimator);

            rootVisualElement.Add(toolbar);
        }

        private void Export()
        {
            List<Node> nodes = _graphView.nodes.ToList();
            List<AnimationClip> clips = new List<AnimationClip>();
            Dictionary<DirectionBillboardNode, AnimatorStateMachine> nodesStates = new Dictionary<DirectionBillboardNode, AnimatorStateMachine>();
            Dictionary<DirectionBillboardNode, List<AnimatorState>> nodesStatesAnim = new Dictionary<DirectionBillboardNode, List<AnimatorState>>();
            //Create Animation
            if(nodes.Count == 0)
            {
                EditorUtility.DisplayDialog("No Node","No Node to convert", "I Understand");
                return;
            }
            foreach (DirectionBillboardNode node in nodes)
            {
                if(node.animation == null)
                {
                    EditorUtility.DisplayDialog("No animation", "Some node don't have animation", "I Understand");
                    return;
                }
                foreach(DirectionalSprite.Direction direction in node.animation.directions)
                {
                    AnimationClip clip = new AnimationClip();
                    clip.name = node.animation.name + "_" + direction.angleStart + "_" + direction.angleEnd;
                    clip.wrapMode = node.animation.loop ? WrapMode.Loop : WrapMode.Default;
                    AnimationClipSettings settings = new AnimationClipSettings();
                    settings.loopTime = node.animation.loop;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    clip.ClearCurves();
                    if(direction.sprites.Count != 0)
                    {
                        EditorCurveBinding curveBinding = new EditorCurveBinding();
                        curveBinding.type = typeof(SpriteRenderer);
                        curveBinding.path = "";
                        curveBinding.propertyName = "m_Sprite";
                        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[direction.sprites.Count];
                        for (int i = 0; i < direction.sprites.Count; i++)
                        {
                            Sprite sprite = direction.sprites[i];
                            keyFrames[i] = new ObjectReferenceKeyframe();
                            keyFrames[i].time = i * node.animation.waitTime;
                            keyFrames[i].value = sprite;

                        }
                        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
                    }
                    clips.Add(clip);
                    //Coucou
                }
            }

            string path = EditorUtility.SaveFilePanelInProject("Export Animations", "animations", "", "Export Animations");
            if(path == "")
                return;

            foreach(AnimationClip clip in clips)
            {
                string clipPath = path + clip.name + ".anim";
                AssetDatabase.CreateAsset(clip, clipPath);
            }


            AssetDatabase.SaveAssets();

            //Create Animator
            AnimatorController controller = null;
            AnimatorStateMachine rootStateMachine = null;

            if (clips.Count != 0)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path + ".controller");
                controller.AddParameter("angle", AnimatorControllerParameterType.Float);
                rootStateMachine = controller.layers[0].stateMachine;

            }

            for (int i = 0; i < nodes.Count; i++)
            {
                DirectionBillboardNode node = (DirectionBillboardNode)nodes[i];
                float stateRadian = (((float)i / (nodes.Count)) * 360f) * Mathf.Deg2Rad;
                float stateX = Mathf.Cos(stateRadian); ;
                float stateY = Mathf.Sin(stateRadian); ;
                Vector3 statePos = Vector3.zero;
                statePos += new Vector3(stateX, stateY) * 300;

                AnimatorStateMachine stateMachine = rootStateMachine.AddStateMachine(node.animation.name, statePos);
                controller.AddParameter(node.animation.name, AnimatorControllerParameterType.Trigger);
                AnimatorState baseState = stateMachine.AddState("Base", new Vector3(0,  600));
                nodesStates.Add(node, stateMachine);
                List<AnimatorState> states = new List<AnimatorState>();
                for (int x = 0; x < node.animation.directions.Count; x++)
                {
                    DirectionalSprite.Direction direction = node.animation.directions[x];
                    string name = node.animation.name + "_" + direction.angleStart + "_" + direction.angleEnd;
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path + name + ".anim");
                    
                    float radians = (((float)x/(node.animation.directions.Count))*360f) * Mathf.Deg2Rad;
                    var cos = Mathf.Cos(radians);
                    var sin = Mathf.Sin(radians);
                    Vector3 pos = new Vector3(0, 600);
                    pos+= new Vector3(cos, sin) * 300;

                    AnimatorState state = stateMachine.AddState(name, pos);
                    state.motion = clip;
                    states.Add(state);
                    var transition = AddTransition(state ,baseState);
                    transition.AddCondition(AnimatorConditionMode.Greater, direction.angleEnd, "angle");
                    
                    transition = AddTransition(state, baseState);
                    transition.AddCondition(AnimatorConditionMode.Less,direction.angleStart,"angle");


                    transition = AddTransition(baseState ,state);
                    transition.AddCondition(AnimatorConditionMode.Greater, direction.angleStart, "angle");
                    transition.AddCondition(AnimatorConditionMode.Less, direction.angleEnd, "angle");
                }
                nodesStatesAnim.Add(node, states);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                DirectionBillboardNode node = (DirectionBillboardNode)nodes[i];
                AnimatorStateMachine stateMachine = null;
                nodesStates.TryGetValue(node, out stateMachine);

                for (int y = 0; y < node.ports.Count; y++)
                {
                    Port port = node.ports[y];
                    if(port.direction == UnityEditor.Experimental.GraphView.Direction.Input)
                    {
                        continue;
                    }
                    List<Edge> edges = port.connections.ToList();
                    List<AnimatorState> states = new List<AnimatorState>();
                    nodesStatesAnim.TryGetValue(node, out states);
                    for (int x = 0; x < edges.Count; x++)
                    {
                        DirectionBillboardNode endNode = (DirectionBillboardNode)edges[x].input.node;
                        if (endNode == port.node)
                        {
                            continue;
                        }
                        AnimatorStateMachine endStateMachine = null;
                        nodesStates.TryGetValue(endNode, out endStateMachine);
                        var transition = rootStateMachine.AddStateMachineTransition(stateMachine ,endStateMachine);
                        transition.AddCondition(AnimatorConditionMode.If, 1, endNode.animation.name);

                        foreach(AnimatorState state in states)
                        {
                            AnimatorStateTransition trans = state.AddExitTransition();
                            trans.duration = 0;
                            trans.hasExitTime = false;
                            trans.AddCondition(AnimatorConditionMode.If, 1, endNode.animation.name);
                        }
                    }
                }

            }
            AssetDatabase.SaveAssets();


        }

        public AnimatorStateTransition AddTransition(AnimatorState baseState, AnimatorState endState)
        {
            var transition = baseState.AddTransition(endState);
            transition.duration = 0;
            transition.hasExitTime = false;
            return transition;
        }

        protected void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        private void OnGUI()
        {
            DragAndDropGUI();
        }

        private void DragAndDropGUI()
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dragAndDropBox.contentRect.Contains(evt.mousePosition))
                    {
                        foreach(UnityEngine.Object obj in DragAndDrop.objectReferences)
                        {
                            if (!(obj is DirectionalBillboard))
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                                return;
                            }
                        }
                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                            {
                                _graphView.CreateNode((DirectionalBillboard)obj, evt.mousePosition);
                            }
                        }
                    }
                    break;
            }

        }
    }
}