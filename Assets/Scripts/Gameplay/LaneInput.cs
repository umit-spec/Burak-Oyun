using UnityEngine;
using UnityEngine.InputSystem;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Input soyutlaması. MVP: klavye (ok tuşları + A/D).
    /// Android'e geçişte buraya swipe eklenir, SnakeController değişmez.
    /// </summary>
    public class LaneInput : MonoBehaviour
    {
        /// -1 = sola geç, +1 = sağa geç, 0 = yok. Her frame sıfırlanır.
        public int ConsumeLaneChange()
        {
            int dir = pendingDir;
            pendingDir = 0;
            return dir;
        }

        private int pendingDir;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                pendingDir = -1;
            else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                pendingDir = 1;
        }
    }
}
