using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class VertexGroup
    {
        public Vector3 Vertex;
        public int VertixIndex;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Color VertexColor;
        public bool IsAlignment;

        public static implicit operator VertexGroup(Vector3 vertix)
        {
            var vertixGroup = new VertexGroup
            {
                Vertex = vertix
            };
            return vertixGroup;
        }

        public static VertexGroup GetAlignmentVertex()
        {
            VertexGroup group = Vector3.zero;
            group.IsAlignment = true;
            group.Tangent = Vector4.zero;
            group.Normal = Vector3.one;
            return group;
        }
    }
}
