using Com.DiazTeo.DirectionalSprite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSpriteEditor
{
    public class GraphEditorWindows : EditorWindow
    {

        static protected GraphEditorWindows window;

        protected static DirectionBillboardGraph graph;
        protected static DirectionBillboardGraphGUI graphGUI;

        [MenuItem("Tools/DirectionalSprite/ConvertToAnimator")]
        protected static void Init()
        {
             
            window = (GraphEditorWindows)EditorWindow.GetWindow(typeof(GraphEditorWindows));
            window.CreateGraph();
            window.Show();
            
        }

        public void CreateGraph()
        {
            graph = ScriptableObject.CreateInstance<DirectionBillboardGraph>();
            graph.hideFlags = HideFlags.HideAndDontSave;

            CreateNode("mile2", new Rect(400,34,500,250));
            CreateNode("mile1", new Rect(0,0,300,200));

            graphGUI = ScriptableObject.CreateInstance<DirectionBillboardGraphGUI>();
            graphGUI.graph = graph;

        }

        public void CreateNode(string title, Rect position)
        {
            //create new node
            Node node = ScriptableObject.CreateInstance<Node>();
            node.title = title;
            node.position = position;

            node.AddInputSlot("input");
            node.AddOutputSlot("output");
            node.AddProperty(new Property(typeof(System.Int32), "integer"));
            graph.AddNode(node);
        }

        private void OnGUI()
        {
            graphGUI.BeginGraphGUI(window, new Rect(0, 0, window.position.width, window.position.height));
            DropAreaGUI();
            graphGUI.OnGraphGUI();
            graphGUI.EndGraphGUI();
        }

        private void DropAreaGUI()
        {
            Event evt = Event.current;
            Rect area = new Rect(0, 0, window.position.width, window.position.height);
            GUI.Box(area, new GUIContent());
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!area.Contains(evt.mousePosition))
                    {
                        return;
                    }
                    if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is DirectionalBillboard)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            Debug.Log("Drag");
                            CreateNode(DragAndDrop.objectReferences[0].name, new Rect(evt.mousePosition.x, evt.mousePosition.y, 300, 200));
                        }
                        return;
                    }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    break;

            }
        }
    }
}
