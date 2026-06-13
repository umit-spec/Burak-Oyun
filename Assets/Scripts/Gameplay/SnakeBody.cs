using System.Collections.Generic;
using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Saf C# yılan gövdesi — ızgara konumlarının listesi ([0] = baş). Unit-test edilebilir.
    /// Görsel temsil SnakeController'dadır; burada GameObject yoktur.
    /// </summary>
    public class SnakeBody
    {
        private readonly List<Vector2Int> segments = new();

        public IReadOnlyList<Vector2Int> Segments => segments;
        public int Length => segments.Count;
        public Vector2Int HeadPosition => segments[0];
        public Vector2Int TailPosition => segments[segments.Count - 1];

        /// <summary>Yılanı baştan kurar; gövde, bakış yönünün TERSİNE doğru uzanır.</summary>
        public void Reset(Vector2Int head, Direction facing, int length)
        {
            segments.Clear();
            Vector2Int back = -GridMovement.ToVector(facing);
            for (int i = 0; i < length; i++)
                segments.Add(head + back * i);
        }

        /// <summary>Başı yeni hücreye taşır. Büyüme = kuyruğu silmeden çağırmak.</summary>
        public void AdvanceHead(Vector2Int newHead) => segments.Insert(0, newHead);

        public void RemoveTail() => segments.RemoveAt(segments.Count - 1);

        public bool ContainsPosition(Vector2Int pos) => segments.Contains(pos);

        /// <summary>
        /// Kuyruk hücresi HARİÇ gövdede mi? Büyümeyen tick'te kuyruk boşalacağı için
        /// o hücreye girmek güvenlidir (klasik yılan kuralı — çocuk dostu, daha bağışlayıcı).
        /// </summary>
        public bool ContainsPositionExceptTail(Vector2Int pos)
        {
            for (int i = 0; i < segments.Count - 1; i++)
                if (segments[i] == pos) return true;
            return false;
        }
    }
}
