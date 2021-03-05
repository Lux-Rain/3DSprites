using Com.DiazTeo.DirectionalSprite;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Com.DiazTeo.DirectionalSpriteEditor
{
    public class DirectionBillboardNode : Node
    {
        public string GUID;
        public DirectionalBillboard animation;
        public List<Port> ports = new List<Port>();
        public DirectionBillboardNode()
        {

        }
    }
}