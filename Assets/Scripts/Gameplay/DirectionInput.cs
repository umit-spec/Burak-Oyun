using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Input soyutlaması: klavye (WASD + ok tuşları) + dokunmatik (parmakla kaydırma).
    /// Hızlı basışlar kuyruğa alınır (en fazla 2) → tick'ler arası girişler kaybolmaz,
    /// köşe dönüşleri (örn. yukarı+sağ peş peşe) güvenilir çalışır.
    /// Ters yön güvenliği TryGetNextDirection'da; swipe de aynı Enqueue'dan geçtiği için
    /// dokunmatik girişte de yılan asla anında kendine dönmez.
    /// </summary>
    public class DirectionInput : MonoBehaviour
    {
        private const int MaxQueued = 2;
        private readonly Queue<Direction> queued = new();

        [Tooltip("Kaydırmanın yön sayılması için gereken minimum parmak mesafesi (piksel). Küçük = daha hassas.")]
        [SerializeField] private float minSwipePixels = 50f;

        private Vector2 swipeStart;
        private bool swiping;

        private void Update()
        {
            ReadKeyboard();
            ReadTouch();
        }

        private void ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Enqueue(Direction.Up);
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Enqueue(Direction.Down);
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) Enqueue(Direction.Left);
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) Enqueue(Direction.Right);
        }

        private void ReadTouch()
        {
            var ts = Touchscreen.current;
            if (ts == null) return;
            var touch = ts.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                swipeStart = touch.position.ReadValue();
                swiping = true;
            }
            else if (swiping && touch.press.wasReleasedThisFrame)
            {
                swiping = false;
                Vector2 delta = touch.position.ReadValue() - swipeStart;
                if (TrySwipeToDirection(delta, minSwipePixels, out Direction dir)) Enqueue(dir);
            }
        }

        /// <summary>
        /// Saf yardımcı (Unity'siz, test edilebilir): kaydırma vektörünü yöne çevirir.
        /// Baskın eksene göre karar verir; eşik altı kaydırma "yön değil" sayılır (yanlışlıkla dokunma).
        /// Ekran +Y yukarı = Direction.Up (ızgara +Y ile tutarlı).
        /// </summary>
        public static bool TrySwipeToDirection(Vector2 delta, float minMagnitude, out Direction dir)
        {
            dir = Direction.Right;
            if (delta.magnitude < minMagnitude) return false;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                dir = delta.x >= 0f ? Direction.Right : Direction.Left;
            else
                dir = delta.y >= 0f ? Direction.Up : Direction.Down;
            return true;
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
