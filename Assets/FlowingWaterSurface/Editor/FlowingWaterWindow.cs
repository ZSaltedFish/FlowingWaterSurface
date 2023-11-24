using System.Collections.Generic;
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

        private List<Mesh> _meshes = new List<Mesh>();
        private GameObject _meshGoRoot;

        public bool ShowVertexDetail = false;

        #region ��ʼ����ر�
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
            foreach (var mesh in _meshes)
            {
                DestroyImmediate(mesh);
            }

            if (_meshGoRoot)
            {
                DestroyImmediate(_meshGoRoot);
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
                    foreach (var mesh in _meshes)
                    {
                        DestroyImmediate(mesh);
                    }

                    if (_meshGoRoot)
                    {
                        DestroyImmediate(_meshGoRoot);
                    }
                    _meshGoRoot = new GameObject("River Root")
                    {
                        hideFlags = HideFlags.DontSave
                    };
                    var divisions = _list.CreateMesh(0.1f, VertexLerp);
                    foreach (var division in divisions)
                    {
                        CreateMeshObject(division);
                    }
                }
            }
        }

        private void CreateMeshObject(MeshDivision division)
        {
            var go = new GameObject(division.SrcMesh.name)
            {
                hideFlags = HideFlags.DontSave
            };
            go.transform.SetParent(_meshGoRoot.transform, false);
            go.transform.position = division.SrcPosition;
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            filter.sharedMesh = division.SrcMesh;
            renderer.sharedMaterial = RiverMaterial;
            _meshes.Add(division.SrcMesh);
        }
        #endregion

        #region ÿ֡����
        public void OnGUI()
        {
            DrawActiveTitle();

            ShowVertexDetail = EditorGUILayout.Toggle("Show vertex detail", ShowVertexDetail);
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

            if (ShowVertexDetail)
            {
                var childCount = _meshGoRoot.transform.childCount;
                for (var i = 0; i < childCount; ++i)
                {
                    var child = _meshGoRoot.transform.GetChild(i);
                    var mesh = child.GetComponent<MeshFilter>().sharedMesh;
                    var vertices = mesh.vertices;
                    for (var j = 0; j < vertices.Length; ++j)
                    {
                        // draw tangent
                        var pos = child.TransformPoint(vertices[j]);
                        var tan = mesh.tangents[j];
                        var tanPos = pos + child.TransformDirection(tan) * 0.1f;
                        Handles.color = Color.red;
                        Handles.DrawLine(pos, tanPos);

                        // draw normal
                        var normal = mesh.normals[j];
                        var normalPos = pos + child.TransformDirection(normal) * 0.1f;
                        Handles.color = Color.green;
                        Handles.DrawLine(pos, normalPos);
                    }
                }
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
