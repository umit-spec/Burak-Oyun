using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace BurakOyun.Gameplay
{
    /// 4-yönlü giriş: klavye (W/A/S/D + ok tuşları) ve dokunmatik kaydırma.
    /// Menü / duraklatma / oyun-bitti durumunda input okunmaz; UI butonları çalışsın.
    [RequireComponent(typeof(SnakeController))]
    public class LaneInput : MonoBehaviour
    {
        [SerializeField] private float swipeThreshold = 0.06f;

        private SnakeController snake;
        private Vector2 touchStart;
        private bool touchStartedOverUI;

        private void Awake() => snake = GetComponent<SnakeController>();

        private void Update()
        {
            if (snake != null && !snake.IsMoving) return;
            ReadKeyboard();
            ReadTouch();
        }

        void ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if      (kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame)
                snake.SetDirection(Vector2Int.up);
            else if (kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame)
                snake.SetDirection(Vector2Int.down);
            else if (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame)
                snake.SetDirection(Vector2Int.left);
            else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                snake.SetDirection(Vector2Int.right);
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
                touchStartedOverUI = IsOverUI(fingerId);
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                if (touchStartedOverUI) { touchStartedOverUI = false; return; }

                Vector2 end = ts.primaryTouch.position.ReadValue();
                float dx = end.x - touchStart.x;
                float dy = end.y - touchStart.y;
                float minSwipe = Screen.width * swipeThreshold;

                if (Mathf.Abs(dx) < minSwipe && Mathf.Abs(dy) < minSwipe)
                {
                    touchStartedOverUI = false;
                    return;
                }

                if (Mathf.Abs(dx) >= Mathf.Abs(dy))
                    snake.SetDirection(dx > 0 ? Vector2Int.right : Vector2Int.left);
                else
                    snake.SetDirection(dy > 0 ? Vector2Int.up : Vector2Int.down);

                touchStartedOverUI = false;
            }
        }

        private static bool IsOverUI(int fingerId)
        {
            if (EventSystem.current == null) return false;
            if (EventSystem.current.IsPointerOverGameObject(fingerId)) return true;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
