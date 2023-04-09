using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.ZKnight.FlowingWaterSurface.Editor
{
    public class FlowingWaterWindow : EditorWindow
    {
        public Vector3 Size = Vector3.one * 0.1f;
        private GameObject _root;
        public GameObject RootObject
        {
            get
            {
                if (_root == null)
                {
                    _root = new GameObject("Root")
                    {
                        hideFlags = HideFlags.DontSave
                    };
                    _root.transform.position = Vector3.zero;
                    _root.transform.localScale = Vector3.one;
                    _root.transform.eulerAngles = Vector3.zero;
                }

                return _root;
            }
        }

        private List<GameObject> _mainList;

        #region 初始化与关闭
        public void Awake()
        {
            _mainList = new List<GameObject>();
        }

        public void OnEnable()
        {
            SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
            SceneView.beforeSceneGui += SceneView_beforeSceneGui;
        }

        public void OnDisable()
        {
            SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
        }

        public void OnDestroy()
        {
            foreach (var go in _mainList)
            {
                DestroyImmediate(go);
            }

            if (_root != null)
            {
                DestroyImmediate(_root);
            }
        }

        private void SceneView_beforeSceneGui(SceneView obj)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out var hitInfo, 1000f, LayerMask.GetMask("Terrain", "Default")))
                {
                    var go = CreateMainListObject(hitInfo.point, Size);
                    _mainList.Add(go);
                }
                e.Use();
            }
        }
        #endregion

        public void OnGUI()
        {
            Size = EditorGUILayout.Vector3Field("Size", Size);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var index = 0;
                foreach (var go in _mainList)
                {
                    EditorGUILayout.ObjectField($"Item{index}", go, typeof(GameObject), false);
                    ++index;
                }
            }
        }

        private GameObject CreateMainListObject(Vector3 pos, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(RootObject.transform, false);
            go.transform.position = pos;
            go.transform.eulerAngles = Vector3.zero;
            go.transform.localScale = size;
            DestroyImmediate(go.GetComponent<Collider>());

            Debug.Log($"Create Cube at {pos}");
            return go;
        }

        [MenuItem("Tools/FlowingWaterSurface Window")]
        public static void Init()
        {
            var window = GetWindow<FlowingWaterWindow>();
            window.Show();
        }
    }
}
