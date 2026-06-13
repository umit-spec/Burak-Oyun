using System.Collections.Generic;
using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// Her doğru harfte bir segment eklenir; segmentler birbirini takip eder.
    public class SnakeTail : MonoBehaviour
    {
        [SerializeField] private float segmentSpacing = 0.65f;

        private readonly List<Transform> segments = new();
        private static readonly Color HeadColor  = new(0.2f, 0.85f, 0.3f);
        private static readonly Color TailColor  = new(0.1f, 0.45f, 0.15f);

        public int Count => segments.Count;

        public void AddSegment()
        {
            var seg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            seg.name = $"Tail_{segments.Count + 1}";

            // Spawn behind last segment (or head)
            Transform anchor = segments.Count > 0 ? segments[^1] : transform;
            seg.transform.position = anchor.position - anchor.forward * segmentSpacing;
            seg.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            float t = Mathf.Clamp01(segments.Count / 8f);
            float scale = Mathf.Lerp(0.75f, 0.45f, t);
            seg.transform.localScale = new Vector3(scale, scale, scale);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.Lerp(HeadColor, TailColor, t);
            seg.GetComponent<Renderer>().sharedMaterial = mat;

            Object.Destroy(seg.GetComponent<CapsuleCollider>());
            segments.Add(seg.transform);
        }

        public void ClearSegments()
        {
            foreach (var s in segments)
                if (s != null) Destroy(s.gameObject);
            segments.Clear();
        }

        private void LateUpdate()
        {
            if (segments.Count == 0) return;

            Transform leader = transform;
            foreach (var seg in segments)
            {
                if (seg == null) continue;
                Vector3 dir = leader.position - seg.position;
                float dist = dir.magnitude;
                if (dist > segmentSpacing)
                    seg.position += dir.normalized * (dist - segmentSpacing);
                if (dir.sqrMagnitude > 0.001f)
                    seg.forward = dir.normalized;
                leader = seg;
            }
        }

        private void OnDestroy() => ClearSegments();
    }
}
