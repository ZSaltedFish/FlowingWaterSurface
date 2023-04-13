using System.Collections.Generic;
using UnityEngine;

namespace com.ZKnight.FlowingWaterSurface.Editor
{
    public static class LinkHelper
    {
        private static Vector3 CatmullBomResult(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var r0 = p0 * (-0.5f * t * t * t + t * t - 0.5f * t);
            var r1 = p1 * (1.5f * t * t * t - 2.5f * t * t + 1.0f);
            var r2 = p2 * (-1.5f * t * t * t + 2.0f * t * t + 0.5f * t);
            var r3 = p3 * (0.5f * t * t * t - 0.5f * t * t);

            return r0 + r1 + r2 + r3;
        }

        public static List<Vector3> CatmullBomSpline(List<Vector3> points, float step = 0.1f)
        {
            if (points.Count <= 1) return points;

            var list = new List<Vector3>(points);
            list.Insert(0, 2 * points[0] - points[1]);
            list.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            var result = new List<Vector3>();
            for (int index = 1; index < list.Count - 2; ++index)
            {
                for (float stepValue = 0; stepValue < 1; stepValue += step)
                {
                    var p0 = list[index - 1];
                    var p1 = list[index];
                    var p2 = list[index + 1];
                    var p3 = list[index + 2];

                    var resultPoint = CatmullBomResult(stepValue, p0, p1, p2, p3);
                    result.Add(resultPoint);
                }
            }

            return result;
        }
    }
}
