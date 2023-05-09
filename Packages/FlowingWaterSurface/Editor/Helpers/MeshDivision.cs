using System.Collections.Generic;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class MeshDivision
    {
        public Mesh SrcMesh;
        public List<VerticesRowGroup> SubRows;
        public Vector3 SrcPosition;

        public MeshDivision(string meshName, List<VerticesRowGroup> subRows)
        {
            SrcPosition = subRows[0].MidVertex.Vertex;
            SrcMesh = new Mesh()
            {
                hideFlags = HideFlags.DontSave,
                name = meshName
            };
            SubRows = subRows;
        }

        public void Run()
        {
            ResetIndex();
            SetVertex(ref SrcMesh);
            SetTriangle(ref SrcMesh);
        }

        private void ResetIndex()
        {
            var index = 0;
            foreach (var row in SubRows)
            {
                index = row.SetIndices(index);
            }
        }

        private void SetVertex(ref Mesh mesh)
        {
            var groups = new List<VertexGroup>();
            foreach (var rowGroup in SubRows)
            {
                groups.AddRange(rowGroup.VertexGroupList);
            }

            var vertexList = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();

            foreach (var group in groups)
            {
                vertexList.Add(group.Vertex - SrcPosition);
                normals.Add(group.Normal);
                tangents.Add(group.Tangent);
            }

            mesh.SetVertices(vertexList);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
        }

        private void SetTriangle(ref Mesh mesh)
        {
            var triangleList = new List<int>();
            for (int index = 0; index < SubRows.Count - 1; ++index)
            {
                var rowGroup = SubRows[index];
                var next = SubRows[index + 1];
                triangleList.AddRange(rowGroup.GenerateTriangle(next));
            }
            mesh.SetTriangles(triangleList.ToArray(), 0);
        }
    }
}
