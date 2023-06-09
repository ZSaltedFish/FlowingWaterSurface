﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{
    public class VerticesMap
    {
        public const int MAX_VERTEX = 64000;
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

        public List<MeshDivision> CreateMesh()
        {
            var list = new List<MeshDivision>();
            var curCount = 0;
            var startIndex = 0;
            for (var index = 0; index < _rows.Count; ++index)
            {
                var row = _rows[index];
                curCount += row.TotalCount;
                if (curCount > MAX_VERTEX)
                {
                    var division = _rows.GetRange(startIndex, index - startIndex);
                    list.Add(new MeshDivision($"River mesh {list.Count}", division));
                    curCount = 0;
                    --index;
                    startIndex = index;
                }
            }
            var lastDivision = _rows.GetRange(startIndex, _rows.Count - startIndex);
            list.Add(new MeshDivision($"River mesh {list.Count}", lastDivision));

            foreach (var division in list)
            {
                division.Run();
            }
            return list;
        }
    }
}
