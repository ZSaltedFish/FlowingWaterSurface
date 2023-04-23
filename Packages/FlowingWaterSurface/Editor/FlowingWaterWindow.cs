using UnityEditor;
using UnityEngine;

namespace ZKnight.FlowingWaterSurface.Editor
{

    public class FlowingWaterWindow : EditorWindow
    {
        public Vector3 Size = Vector3.one * 0.1f;
        public Material RiverMaterial;
        public float VertexLerp = 0.05f;
        private ReorderableGameObjectList _list;
        private GUIStyle _lostStyles, _hasFocus;
        private bool _isUsing;

        private Mesh _srcMesh;
        private GameObject _meshGo;

        #region 初始化与关闭
        public void Awake()
        {
            _list = new ReorderableGameObjectList("Surface Root");
            _hasFocus = new GUIStyle("Button");
            _lostStyles = new GUIStyle("Button");

            _hasFocus.normal.textColor = new Color(0.33f, 0.9f, 0.33f);
            _lostStyles.normal.textColor = new Color(0.9f, 0.33f, 0.33f);
        }

        public void OnEnable()
        {
            SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
            SceneView.beforeSceneGui += SceneView_beforeSceneGui;

            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }

        public void OnDisable()
        {
            SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
        }

        public void OnDestroy()
        {
            if (_srcMesh)
            {
                DestroyImmediate(_srcMesh);
            }

            if (_meshGo)
            {
                DestroyImmediate(_meshGo);
            }
            _list.Dispose();
        }

        private void SceneView_beforeSceneGui(SceneView obj)
        {
            var e = Event.current;
            if (_isUsing && e.type == EventType.MouseDown && e.button == 0)
            {
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out var hitInfo, 1000f, LayerMask.GetMask("Terrain", "Default")))
                {
                    _list.AddGameObject(hitInfo.point, Size);
                    Repaint();
                }
                e.Use();
            }
        }

        private void DrawActiveTitle()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(30f));
            if (_isUsing)
            {
                var title = "Adding Point Mode";
                if (GUI.Button(rect, title, _hasFocus))
                {
                    _isUsing = false;
                }
            }
            else
            {
                var title = "Not using";
                if (GUI.Button(rect, title, _lostStyles))
                {
                    _isUsing = true;
                }

                if (GUILayout.Button("Create mesh", _hasFocus))
                {
                    if (!_meshGo)
                    {
                        _meshGo = new GameObject("Mesh Go");
                        _meshGo.AddComponent<MeshFilter>();
                        _meshGo.AddComponent<MeshRenderer>();
                        _meshGo.hideFlags = HideFlags.DontSave;
                    }

                    if (_srcMesh)
                    {
                        DestroyImmediate(_srcMesh);
                    }
                    _srcMesh = new Mesh
                    {
                        name = "River mesh",
                        hideFlags = HideFlags.DontSave
                    };
                    _list.CreateMesh(0.1f, VertexLerp, ref _srcMesh);
                    _meshGo.GetComponent<MeshFilter>().sharedMesh = _srcMesh;
                    _meshGo.GetComponent<MeshRenderer>().sharedMaterial = RiverMaterial;
                }
            }
        }
        #endregion

        #region 每帧更新
        public void OnGUI()
        {
            DrawActiveTitle();

            RiverMaterial = EditorGUILayout.ObjectField("River material", RiverMaterial, typeof(Material), false) as Material;
            using (new EditorGUILayout.VerticalScope())
            {
                VertexLerp = EditorGUILayout.FloatField("Vertex Lerp", VertexLerp);
                Size = EditorGUILayout.Vector3Field("Size", Size);
            }
            _list.DoLayout();
        }

        #endregion

        #region Gizmos
        private void SceneView_duringSceneGui(SceneView obj)
        {
            var points = _list.GetCatmullBomPoints(VertexLerp);
            if (points.Count > 2)
            {
                Handles.DrawAAPolyLine(points.ToArray());
            }

            var rightPoints = _list.GetRightCatmullBomPoints(VertexLerp);
            if (rightPoints.Count > 2)
            {
                Handles.DrawAAPolyLine(rightPoints.ToArray());
            }
            var leftPoints = _list.GetLeftCatmullBomPoints(VertexLerp);
            if (leftPoints.Count > 2)
            {
                Handles.DrawAAPolyLine(leftPoints.ToArray());
            }
        }
        #endregion

        [MenuItem("Tools/FlowingWaterSurface Window")]
        public static void Init()
        {
            var window = GetWindow<FlowingWaterWindow>();
            window.Show();
        }
    }
}
