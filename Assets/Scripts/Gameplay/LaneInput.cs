using UnityEngine;
using UnityEngine.InputSystem;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Input soyutlaması: klavye (PC) + dokunmatik (Android).
    /// Dokunmatik: ekranın sol/sağ yarısına tap VEYA sola/sağa swipe.
    /// SnakeController bu sınıfı bilmez — sadece ConsumeLaneChange() çağırır.
    /// </summary>
    public class LaneInput : MonoBehaviour
    {
        // Swipe olarak sayılacak minimum yatay piksel mesafesi (ekran genişliğinin ~%8'i).
        // Bu değerin altındaki harekete tap muamelesi yapılır.
        [SerializeField] private float swipeThreshold = 0.08f; // Screen.width ile çarpılır

        /// -1 = sola geç, +1 = sağa geç, 0 = yok. Her frame sıfırlanır.
        public int ConsumeLaneChange()
        {
            int dir = pendingDir;
            pendingDir = 0;
            return dir;
        }

        private int pendingDir;
        private Vector2 touchStart;

        private void Update()
        {
            ReadKeyboard();
            ReadTouch();
        }

        void ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                pendingDir = -1;
            else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                pendingDir = 1;
        }

        void ReadTouch()
        {
            var ts = Touchscreen.current;
            if (ts == null) return;

            var phase = ts.primaryTouch.phase.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStart = ts.primaryTouch.position.ReadValue();
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                Vector2 end = ts.primaryTouch.position.ReadValue();
                float dx = end.x - touchStart.x;
                float threshold = Screen.width * swipeThreshold;

                // Swipe ise yönüne, tap ise ekranın hangi yarısına basıldığına bak
                pendingDir = Mathf.Abs(dx) >= threshold
                    ? (dx > 0 ? 1 : -1)
                    : (touchStart.x < Screen.width * 0.5f ? -1 : 1);
            }
        }
    }
}
