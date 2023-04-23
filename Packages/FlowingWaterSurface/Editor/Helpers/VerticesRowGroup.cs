using System.Collections.Generic;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class VerticesRowGroup
    {
        public List<VertexGroup> VertexGroups;
        public int StartIndex => _startIndex;
        public int EndIndex => _endIndex;
        public int MidIndex => _startIndex + _leftCount;
        public int TotalCount => _leftCount + _rightCount + 1;
        public VertexGroup StartVertex => VertexGroups[_startIndex];
        public VertexGroup EndVertex => VertexGroups[_endIndex];
        public VertexGroup MidVertex => VertexGroups[MidIndex];
        public int LeftCount => _leftCount;
        public int RightCount => _rightCount;

        private float _distance;
        private int _leftCount, _rightCount;
        private int _startIndex = 0, _endIndex = 0;

        public VertexGroup this[int index]
        {
            get { return VertexGroups[index]; }
        }

        public VerticesRowGroup(float distance)
        {
            VertexGroups = new List<VertexGroup>();
            _distance = distance;
        }

        #region 数据准备
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void InitialList(Vector3 mid, Vector3 left, Vector3 right)
        {
            var dLeft = Vector3.Distance(mid, left);
            var dRight = Vector3.Distance(mid, right);
            _leftCount = Mathf.FloorToInt(dLeft / _distance) + 1;
            _rightCount = Mathf.FloorToInt(dRight / _distance) + 1;

            CreateVertices(mid, left, right, dLeft, dRight);
        }

        /// <summary>
        /// 对齐队列
        /// </summary>
        /// <param name="maxLeft"></param>
        /// <param name="maxRight"></param>
        public void Alignment(int maxLeft, int maxRight)
        {
            var leftAdd = maxLeft - _leftCount;
            for (var i = 0; i < leftAdd; ++i)
            {
                VertexGroups.Insert(0, VertexGroup.GetAlignmentVertex());
                ++_startIndex;
            }

            _endIndex = VertexGroups.Count - 1;
            var rightAdd = maxRight - _rightCount;
            for (var i = 0; i < rightAdd; ++i)
            {
                VertexGroups.Add(VertexGroup.GetAlignmentVertex());
            }
        }

        /// <summary>
        /// 设置顶点索引
        /// </summary>
        /// <param name="src"></param>
        public int SetIndices(int src)
        {
            for (var index = StartIndex; index <= EndIndex; ++index)
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
                float length = EndIndex - StartIndex;
                float lengthNext = next.EndIndex - next.StartIndex;
                for (int index = StartIndex; index <= EndIndex; ++index)
                {
                    var curNext = this[index];
                    var percent = (index - StartIndex) / length;
                    var nextVertexIndex = Mathf.CeilToInt(percent * lengthNext + next.StartIndex);
                    var nextVertex = next[nextVertexIndex];

                    var tangent = (curNext.Vertex - nextVertex.Vertex).normalized;
                    var right = (index == EndIndex) ? (curNext.Vertex - this[MidIndex].Vertex).normalized : (this[index + 1].Vertex - curNext.Vertex).normalized;
                    var normal = Vector3.Cross(right, -tangent).normalized;

                    curNext.Tangent = tangent;
                    curNext.Tangent.w = -1f;
                    curNext.Normal = -normal;
                }
            }
        }

        /// <summary>
        /// 设置最后一排参数
        /// </summary>
        /// <param name="src"></param>
        public void SetLastRowVertexDetial(VerticesRowGroup src)
        {
            float length = EndIndex - StartIndex;
            float lengthSrc = src.EndIndex - src.StartIndex;

            for (int index = StartIndex; index <= EndIndex; ++index)
            {
                var curNext = this[index];
                var percent = (index - StartIndex) / length;
                var srcVertexIndex = Mathf.CeilToInt(percent * lengthSrc + src.StartIndex);
                var srcVertex = src[srcVertexIndex];

                curNext.Tangent = srcVertex.Tangent;
                curNext.Normal = srcVertex.Normal;
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

            var curStart = _startIndex;
            var curEnd = _endIndex;

            var nextStart = next._startIndex;
            var nextEnd = next._endIndex;
            for (var i = Mathf.Min(curStart, nextStart); i <= Mathf.Max(curEnd - 1, nextEnd - 1); ++i)
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
                return VertexGroups.GetRange(StartIndex, TotalCount);
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

        private void CreateVertices(Vector3 mid, Vector3 left, Vector3 right, float dLeft, float dRight)
        {
            var lDire = (mid - left).normalized;
            var rDire = (right - mid).normalized;
            VertexGroups.Add(left);

            var lSp = dLeft / _leftCount;
            for (int i = 0; i < _leftCount - 1; ++i)
            {
                var length = lSp * (i + 1);
                var pos = lDire * length + left;
                VertexGroups.Add(pos);
            }

            VertexGroups.Add(mid);

            var rSp = dRight / _rightCount;
            for (int i = 0; i < _rightCount - 1; ++i)
            {
                var length = rSp * (i + 1);
                var pos = rDire * length + mid;
                VertexGroups.Add(pos);
            }
            VertexGroups.Add(right);
        }
        #endregion
    }
}
