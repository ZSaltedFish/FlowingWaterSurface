using System.Collections.Generic;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class VerticesRowGroup
    {
        public List<VertexGroup> VertexGroups;
        public float Distance => _distance;
        public VertexGroup MidVertex => VertexGroups[_midIndex].Vertex;

        private float _distance;
        private int _midIndex;

        public VertexGroup this[int index]
        {
            get { return VertexGroups[index]; }
        }

        public VerticesRowGroup(float distance)
        {
            VertexGroups = new List<VertexGroup>();
            _distance = distance;
        }

        /// <summary>
        /// 通过计算获取顶点数量
        /// </summary>
        /// <param name="mid">中央顶点</param>
        /// <param name="target">目标顶点</param>
        /// <returns></returns>
        public int GetCount(Vector3 mid, Vector3 target)
        {
            var distance = Vector3.Distance(mid, target);
            var count = Mathf.FloorToInt(distance / _distance) + 1;
            return count;
        }

        public void CreateLeft(Vector3 mid, Vector3 left, int leftCount)
        {
            var dLeft = (mid - left).normalized;
            var distance = Vector3.Distance(mid, left);
            var indi = distance / leftCount;
            for (var i = 0; i < leftCount; ++i)
            {
                var pos = i * indi * dLeft + left;
                VertexGroups.Add(pos);
            }
        }

        public void CreateMiddleVectex(Vector3 mid)
        {
            _midIndex = VertexGroups.Count;
            VertexGroups.Add(mid);
        }

        public void CreateRight(Vector3 mid, Vector3 right, int rightCount)
        {
            var dRight = (right - mid).normalized;
            var distance = Vector3.Distance(mid, right);
            var indi = distance / rightCount;
            for (var i = 1; i <= rightCount; ++i)
            {
                var pos = i * indi * dRight + mid;
                VertexGroups.Add(pos);
            }
        }

        #region 数据准备
        /// <summary>
        /// 设置顶点索引
        /// </summary>
        /// <param name="src"></param>
        public int SetIndices(int src)
        {
            for (var index = 0; index < VertexGroups.Count; ++index)
            {
                this[index].VertixIndex = src;
                ++src;
            }
            return src;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="next"></param>
        public void SetVertexDetial(VerticesRowGroup next)
        {
            if (next != null)
            {
                for (var index = 0; index < VertexGroups.Count; ++index)
                {
                    var curPos = this[index];
                    var nextPos = next[index];

                    var tangent = (nextPos.Vertex - curPos.Vertex).normalized;
                    var right = (index == VertexGroups.Count - 1) ? (curPos.Vertex - this[0].Vertex).normalized : (this[index + 1].Vertex - curPos.Vertex).normalized;
                    var normal = Vector3.Cross(tangent, right).normalized;

                    curPos.Tangent = tangent;
                    curPos.Tangent.w = -1f;
                    curPos.Normal = normal;
                }
            }
        }

        /// <summary>
        /// 设置最后一排参数
        /// </summary>
        /// <param name="src"></param>
        public void SetLastRowVertexDetial(VerticesRowGroup src)
        {
            if (src == null) return;
            for (var index = 0; index < VertexGroups.Count; ++index)
            {
                var curPos = this[index];
                var srcPos = src[index];

                curPos.Tangent = srcPos.Tangent;
                curPos.Normal = srcPos.Normal;
            }
        }
        #endregion

        #region 设置Mesh
        /// <summary>
        /// 生成面
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public List<int> GenerateTriangle(VerticesRowGroup next)
        {
            var triangleList = new List<int>();

            var curStart = 0;
            var curEnd = VertexGroups.Count - 1;

            var nextStart = 0;
            var nextEnd = VertexGroups.Count - 1;
            for (var i = 0; i <= Mathf.Max(curEnd - 1, nextEnd - 1); ++i)
            {
                var curIndex = Mathf.Clamp(i, curStart - 1, curEnd);
                var nextIndex = Mathf.Clamp(i, nextStart - 1, nextEnd);
                var triangles = TriangleSet(VertexGroups[curIndex], VertexGroups[curIndex + 1], next.VertexGroups[nextIndex], next.VertexGroups[nextIndex + 1]);
                triangleList.AddRange(triangles);
            }
            return triangleList;
        }

        public IEnumerable<VertexGroup> VertexGroupList
        {
            get
            {
                return VertexGroups.GetRange(0, VertexGroups.Count);
            }
        }

        private List<int> TriangleSet(VertexGroup cd, VertexGroup cu, VertexGroup nd, VertexGroup nu)
        {
            var list = new List<int>();
            if (cd.IsAlignment)
            {
                ConnectTriangle(list, cu, nu, nd);
            }
            else if (cu.IsAlignment)
            {
                ConnectTriangle(list, cd, nu, nd);
            }
            else if (nu.IsAlignment)
            {
                ConnectTriangle(list, cd, cu, nd);
            }
            else if (nd.IsAlignment)
            {
                ConnectTriangle(list, cd, cu, nu);
            }
            else
            {
                ConnectTriangle(list, cd, nu, nd);
                ConnectTriangle(list, cu, nu, cd);
            }

            return list;
        }

        private static void ConnectTriangle(List<int> list, VertexGroup p0, VertexGroup p1, VertexGroup p2)
        {
            list.Add(p2.VertixIndex);
            list.Add(p1.VertixIndex);
            list.Add(p0.VertixIndex);
        }

        #endregion
    }
}
