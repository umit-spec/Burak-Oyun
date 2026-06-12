using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Input soyutlaması. MVP: klavye (WASD + ok tuşları).
    /// Hızlı basışlar kuyruğa alınır (en fazla 2) → tick'ler arası girişler kaybolmaz,
    /// köşe dönüşleri (örn. yukarı+sağ peş peşe) güvenilir çalışır.
    /// Android'e geçişte buraya swipe eklenir, SnakeController değişmez.
    /// </summary>
    public class DirectionInput : MonoBehaviour
    {
        private const int MaxQueued = 2;
        private readonly Queue<Direction> queued = new();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Enqueue(Direction.Up);
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Enqueue(Direction.Down);
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) Enqueue(Direction.Left);
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) Enqueue(Direction.Right);
        }

        /// <summary>Dışarıdan yön basışı (testler, ileride dokunmatik).</summary>
        public void Enqueue(Direction dir)
        {
            if (queued.Count < MaxQueued) queued.Enqueue(dir);
        }

        /// <summary>
        /// Kuyruktan geçerli ilk yönü verir: mevcut yönün aynısı ve TERSİ atlanır
        /// (ters yön = anında kendine çarpma; tuş spam'i yılanı öldürmemeli).
        /// </summary>
        public bool TryGetNextDirection(Direction current, out Direction next)
        {
            while (queued.Count > 0)
            {
                Direction d = queued.Dequeue();
                if (d == current || GridMovement.AreOpposite(d, current)) continue;
                next = d;
                return true;
            }
            next = current;
            return false;
        }

        public void ClearQueue() => queued.Clear();
    }
}
