using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace BurakOyun.Gameplay
{
    /// Input soyutlaması: klavye (PC) + dokunmatik (Android).
    /// Sadece yılan hareket ediyorken (Playing state) input okur.
    /// UI üzerindeki dokunmalar hiçbir zaman oyun inputu olarak işlenmez.
    [RequireComponent(typeof(SnakeController))]
    public class LaneInput : MonoBehaviour
    {
        [SerializeField] private float swipeThreshold = 0.08f;

        public int ConsumeLaneChange()
        {
            int dir = pendingDir;
            pendingDir = 0;
            return dir;
        }

        private int pendingDir;
        private Vector2 touchStart;
        private bool touchStartedOverUI;
        private SnakeController snake;

        private void Awake()
        {
            snake = GetComponent<SnakeController>();
        }

        private void Update()
        {
            // Menü, pause veya WordComplete durumunda input okuma — UI butonları çalışsın
            if (snake != null && !snake.IsMoving) return;
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
                int fingerId = (int)ts.primaryTouch.touchId.ReadValue();
                // UI üstündeyse bu touch'u oyun inputu sayma
                touchStartedOverUI = IsOverUI(fingerId);
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (touchStartedOverUI) { touchStartedOverUI = false; return; }

                Vector2 end = ts.primaryTouch.position.ReadValue();
                float dx = end.x - touchStart.x;
                float threshold = Screen.width * swipeThreshold;
                pendingDir = Mathf.Abs(dx) >= threshold
                    ? (dx > 0 ? 1 : -1)
                    : (touchStart.x < Screen.width * 0.5f ? -1 : 1);
                touchStartedOverUI = false;
            }
        }

        private static bool IsOverUI(int fingerId)
        {
            if (EventSystem.current == null) return false;
            // Touch pointer check (Android)
            if (EventSystem.current.IsPointerOverGameObject(fingerId)) return true;
            // Mouse/editor fallback
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
