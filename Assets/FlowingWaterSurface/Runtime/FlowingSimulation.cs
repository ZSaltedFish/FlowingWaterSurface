using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.ZKnight.FlowingWaterSurface.Runtime
{
    public class FlowingSimulation : MonoBehaviour
    {
        public GameObject EmissionObject;

        public int MaxEmissionCount = 3600;
        public int EmissionPerSecond = 60;
        public float Size = 0.1f;

        private int _currentEmissteCount => _queue.Count;
        private Queue<GameObject> _queue = new Queue<GameObject>();
        private float _reUp = 0;
        private Dictionary<Vector3Int, int> _position2Count;

        public int CurrentCount = 0;

#if UNITY_EDITOR
        public void Start()
        {
            _position2Count = new Dictionary<Vector3Int, int>();
        }

        public void Update()
        {
            var emCount = (int)(Time.deltaTime * EmissionPerSecond + _reUp);
            if (emCount == 0)
            {
                _reUp += Time.deltaTime * EmissionPerSecond;
            }
            else
            {
                _reUp = 0;
            }

            var remain = DeleteEmiter(emCount);
            for (int i = 0; i < remain; ++i)
            {
                CreateObject();
            }

            CurrentCount = _currentEmissteCount;
        }
#endif

        private int DeleteEmiter(int emCount)
        {
            var remain = MaxEmissionCount - _currentEmissteCount;
            var deleteCount = Mathf.Max(emCount - remain, 0);

            for (int i = 0; i < deleteCount; ++i)
            {
                var go = _queue.Dequeue();
                go.transform.localPosition = Vector3.one;
                go.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _queue.Enqueue(go);
            }

            return emCount - deleteCount;
        }

        private void CreateObject()
        {
            var go = Instantiate(EmissionObject);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * Size;
            go.GetComponent<Rigidbody>().velocity = Vector3.zero;

            _queue.Enqueue(go);
        }
    }
}
