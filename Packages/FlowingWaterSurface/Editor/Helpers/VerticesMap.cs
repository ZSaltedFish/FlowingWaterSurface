using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class VerticesMap
    {
        private List<VerticesRowGroup> _rows;
        private float _distance;

        public VerticesMap(float distance)
        {
            _distance = distance;
            _rows = new List<VerticesRowGroup>();
        }

        public void Generate(List<Vector3> midList, List<Vector3> leftList, List<Vector3> rightList)
        {
            var count = midList.Count;
            var maxSize = 0;
            var maxLeft = 0;
            var maxRight = 0;
            
            for (int i = 0; i < count; ++i)
            {
                var mid = midList[i];
                var left = leftList[i];
                var right = rightList[i];
                var rowGroup = new VerticesRowGroup(_distance);
                _rows.Add(rowGroup);
                rowGroup.InitialList(mid, left, right);
                maxSize = Mathf.Max(maxSize, rowGroup.TotalCount);
                maxLeft = Mathf.Max(maxLeft, rowGroup.LeftCount);
                maxRight = Mathf.Max(maxRight, rowGroup.RightCount);
            }

            var indices = 0;
            foreach (var group in _rows)
            {
                group.Alignment(maxLeft, maxRight);
                indices = group.SetIndices(indices);
            }

            for (int index = 0; index < _rows.Count - 1; ++index)
            {
                _rows[index].SetVertexDetial(_rows[index + 1]);
            }
            _rows[_rows.Count - 1].SetLastRowVertexDetial(_rows[_rows.Count - 2]);
        }

        public void CreateMesh(ref Mesh mesh)
        {
            SetVertex(ref mesh);
            SetTriangle(ref mesh);
        }

        private void SetVertex(ref Mesh mesh)
        {
            var groups = new List<VertexGroup>();
            foreach (var rowGroup in _rows)
            {
                groups.AddRange(rowGroup.VertexGroupList);
            }

            var vertexList = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();

            foreach (var group in groups)
            {
                vertexList.Add(group.Vertex);
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
            for (int index = 0; index < _rows.Count - 1; ++index)
            {
                var rowGroup = _rows[index];
                var next = _rows[index + 1];
                triangleList.AddRange(rowGroup.GenerateTriangle(next));
            }
            mesh.SetTriangles(triangleList.ToArray(), 0);
        }
    }
}
