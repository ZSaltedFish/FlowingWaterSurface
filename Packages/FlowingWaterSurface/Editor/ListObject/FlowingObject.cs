using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.ZKnight.FlowingWaterSurface.Editor
{
    [Serializable]
    public class FlowingObject 
    {
        private GameObject _mainObject, _rightObject, _leftObject;
        private float _rightDistance = 1f, _leftDistance = 1f;
        private FlowingObject _next;

        public GameObject MainObject => _mainObject;
        public GameObject RightObject => _rightObject;
        public GameObject LeftObject => _leftObject;
        public float RightDistance => _rightDistance;
        public float LeftDistance => _leftDistance;
        public Vector3 Position => _mainObject.transform.position;
        public Vector3 LeftPosition => _leftObject.transform.position;
        public Vector3 RightPosition => _rightObject.transform.position;

        public FlowingObject Next
        {
            get => _next;
            set
            {
                _next = value;
                LookAt();
            }
        }

        public FlowingObject(GameObject root, string name, Vector3 pos, Vector3 size)
        {
            CreateMainListObject(root, name, pos, size);
        }

        private void CreateMainListObject(GameObject rootObject, string name, Vector3 pos, Vector3 size)
        {
            _mainObject = CreateObject(rootObject, name, pos, size);

            var leftPos = pos - _mainObject.transform.right * LeftDistance;
            var rightPos = pos + _mainObject.transform.right * RightDistance;

            _rightObject = CreateObject(_mainObject, "Right", rightPos, Vector3.one);
            _leftObject = CreateObject(_mainObject, "Left", leftPos, Vector3.one);
        }

        private GameObject CreateObject(GameObject root, string name, Vector3 pos, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;
            go.transform.eulerAngles = Vector3.zero;
            go.transform.localScale = size;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private void LookAt()
        {
            if (_next == null)
            {
                return;
            }
            var pos = _next.Position;
            var zDire = (pos - Position).normalized;
            var xDire = -Vector3.Cross(zDire, Vector3.up).normalized;
            var yDire = Vector3.Cross(zDire, xDire).normalized;

            _mainObject.transform.LookAt(pos, yDire);

            //_mainObject.transform.forward = zDire;
            //_mainObject.transform.right = xDire;
            //_mainObject.transform.up = yDire;
        }

        public override string ToString()
        {
            return _mainObject.name;
        }

        public void Rotate(Quaternion rotation)
        {
            _mainObject.transform.rotation = rotation;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_mainObject);
        }
    }
}
