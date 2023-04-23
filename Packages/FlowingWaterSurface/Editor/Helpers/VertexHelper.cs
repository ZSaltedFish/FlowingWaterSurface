using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public static class VertexHelper
    {
        public const int MAX_VERTICES = 65000;

        public static List<Vector3> CreateVertex(List<Vector3> points, float step)
        {
            if (points.Count <= 1)
            {
                throw new ArgumentException("少于两个点不能创建Mesh");
            }

            List<Vector3> vertices = LinkHelper.CatmullBomSpline(points, step);
            return vertices;
        }

        public static Mesh GenerateMesh(List<FlowingObject> objList, float step)
        {
            var mesh = new Mesh();
            var listMid = new List<Vector3>();
            var listLeft = new List<Vector3>();
            var listRight = new List<Vector3>();

            foreach (var o in objList)
            {
                listMid.Add(o.Position);
                listLeft.Add(o.LeftPosition);
                listRight.Add(o.RightPosition);
            }

            var verticesMid = CreateVertex(listMid, step);
            var verticesLeft = CreateVertex(listLeft, step);
            var verticesRight = CreateVertex(listRight, step);

            return mesh;
        }
    }
}
