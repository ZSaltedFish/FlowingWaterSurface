using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZKnight.FlowingWaterSurface.Editor
{
    [Serializable]
    public class ReorderableGameObjectList : IDisposable
    {
        private readonly GameObject _root;
        public GameObject RootObject => _root;

        public ReorderableList ReoderList;
        private List<FlowingObject> _goes;

        public ReorderableGameObjectList(string rootName)
        {
            _goes = new List<FlowingObject>();
            ReoderList = new ReorderableList(_goes, typeof(FlowingObject));
            _root = new GameObject(rootName)
            {
                hideFlags = HideFlags.DontSave
            };
            _root.transform.position = Vector3.zero;
            _root.transform.localScale = Vector3.one;
            _root.transform.eulerAngles = Vector3.zero;

            ReoderList.displayAdd = false;
            ReoderList.onRemoveCallback = OnObjectDeleted;
            ReoderList.onReorderCallbackWithDetails = OnReoderList;
            ReoderList.multiSelect = true;
        }

        public void DoLayout()
        {
            ReoderList.DoLayoutList();
        }

        public void AddGameObject(Vector3 pos, Vector3 size)
        {
            var go = new FlowingObject(RootObject, $"Point_{_goes.Count}", pos, size);
            if (_goes.Count > 0)
            {
                var last = _goes[_goes.Count - 1];
                last.Next = go;
                go.Rotate(last.MainObject.transform.rotation);
            }
            _goes.Add(go);
        }

        private void OnObjectDeleted(ReorderableList list)
        {
            var goList = new List<FlowingObject>();
            foreach (var index in list.selectedIndices)
            {
                goList.Add(_goes[index]);
            }

            for (int i = 0; i < goList.Count; ++i) 
            {
                var flowingObject = goList[i];
                _goes.Remove(flowingObject);
                for (int childCount = 0; childCount < _root.transform.childCount; ++childCount)
                {
                    flowingObject.Dispose();
                }
            }
        }

        private void OnReoderList(ReorderableList list, int oldIndex, int newIndex)
        {
            for (int i = 0; i < _goes.Count - 1; ++i)
            {
                var src = _goes[i];
                var dest = _goes[i + 1];
                src.Next = dest;
            }
        }

        public List<Vector3> GetCatmullBomPoints(float step = 0.1f)
        {
            var points = new List<Vector3>();
            foreach (var go in _goes)
            {
                points.Add(go.Position);
            }
            return LinkHelper.CatmullBomSpline(points, step);
        }

        public List<Vector3> GetRightCatmullBomPoints(float step = 0.1f)
        {
            var points = new List<Vector3>();
            foreach (var go in _goes)
            {
                points.Add(go.RightPosition);
            }
            return LinkHelper.CatmullBomSpline(points, step);
        }

        public List<Vector3> GetLeftCatmullBomPoints(float step = 0.1f)
        {
            var points = new List<Vector3>();
            foreach (var go in _goes)
            {
                points.Add(go.LeftPosition);
            }
            return LinkHelper.CatmullBomSpline(points, step);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_root);
        }

        public void CreateMesh(float distance, float step, ref Mesh mesh)
        {
            var map = new VerticesMap(distance);

            var leftList = GetLeftCatmullBomPoints(step);
            var midList = GetCatmullBomPoints(step);
            var rightList = GetRightCatmullBomPoints(step);

            map.Generate(midList, leftList, rightList);
            map.CreateMesh(ref mesh);
        }
    }
}
