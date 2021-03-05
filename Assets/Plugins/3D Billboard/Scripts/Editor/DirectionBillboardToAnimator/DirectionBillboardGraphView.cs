using Com.DiazTeo.DirectionalSprite;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.DiazTeo.DirectionalSpriteEditor
{
    public class DirectionBillboardGraphView : GraphView
    {
        protected Rect defaultNodePosition = new Rect(Vector2.zero, new Vector2(100, 150));
        public DirectionBillboardGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ConvertToAnimatorGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();            
        }

        private Port GeneratePort(DirectionBillboardNode node, UnityEditor.Experimental.GraphView.Direction portDirection, Port.Capacity capacity)
        {
            Port port = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
            node.ports.Add(port);
            return port;
        }

        public void CreateNode(DirectionalBillboard anim, Rect position)
        {
            AddElement(GenerateNode(null, position));

        }
        
        public void CreateNode(DirectionalBillboard anim, Vector2 position)
        {
            Rect rect = new Rect(position, new Vector2(100, 150));
            AddElement(GenerateNode(null, rect));

        }
        
        public void CreateNode(DirectionalBillboard anim)
        {
            AddElement(GenerateNode(null, defaultNodePosition));
        }

        public DirectionBillboardNode GenerateNode(DirectionalBillboard anim, Rect position)
        {
            string nodeName = anim == null ? "Direction Node" : anim.name;
            var node = new DirectionBillboardNode
            {
                title = nodeName,
                GUID = Guid.NewGuid().ToString(),
                animation = anim
            };

            var objectfield = new ObjectField
            {
                label = "Directional Sprites",
                objectType = typeof(DirectionalBillboard),
            };
            objectfield.RegisterValueChangedCallback(evt =>
            {
                node.animation = (DirectionalBillboard)objectfield.value;
                node.title = objectfield.value.name;
            });

            objectfield.value = anim == null ? null : (UnityEngine.Object)anim;

            node.mainContainer.Add(objectfield);

            var port = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi);
            port.portName = "Out";
            node.outputContainer.Add(port);
            port = GeneratePort(node, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Multi);
            port.portName = "In";
            node.inputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(position);
            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                var portView = port;
                if(startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
    }
}