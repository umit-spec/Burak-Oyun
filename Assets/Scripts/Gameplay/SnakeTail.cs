using System.Collections.Generic;
using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// Izgara gövde segmentleri: SnakeController.BodyCells üzerinden her frame senkronize edilir.
    public class SnakeTail : MonoBehaviour
    {
        [SerializeField] private GridBoard board;

        private SnakeController snake;
        private readonly List<GameObject> segments = new();
        private readonly List<Vector2Int> cellBuffer = new();

        private static readonly Color HeadColor = new(0.2f, 0.85f, 0.3f);
        private static readonly Color TailColor  = new(0.05f, 0.4f, 0.12f);

        public int Count => segments.Count;

        private void Awake() => snake = GetComponent<SnakeController>();

        public void ClearSegments()
        {
            foreach (var s in segments)
                if (s != null) Destroy(s);
            segments.Clear();
        }

        private void LateUpdate()
        {
            if (snake == null || board == null) return;

            // Body hücrelerini topla (baş hariç)
            cellBuffer.Clear();
            bool first = true;
            foreach (var c in snake.BodyCells)
            {
                if (first) { first = false; continue; }
                cellBuffer.Add(c);
            }

            // Segment sayısını hücre sayısına eşitle
            while (segments.Count < cellBuffer.Count)
                segments.Add(CreateSegment(segments.Count));
            while (segments.Count > cellBuffer.Count)
            {
                if (segments[^1] != null) Destroy(segments[^1]);
                segments.RemoveAt(segments.Count - 1);
            }

            // Konumları güncelle
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] != null)
                    segments[i].transform.position = board.CellToWorld(cellBuffer[i]) + Vector3.up * 0.45f;
            }
        }

        private GameObject CreateSegment(int idx)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = $"Tail_{idx + 1}";

            var col = g.GetComponent<BoxCollider>();
            if (col != null) Destroy(col);

            float sc = board != null ? board.Cell * 0.82f : 1.2f;
            g.transform.localScale = Vector3.one * sc;

            float t = Mathf.Clamp01(idx / 8f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.Lerp(HeadColor, TailColor, t);
            g.GetComponent<Renderer>().sharedMaterial = mat;

            return g;
        }

        private void OnDestroy() => ClearSegments();
    }
}
