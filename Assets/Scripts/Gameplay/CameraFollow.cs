using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// Izgara oyunu için sabit kuş-bakışı kamera.
    /// Tahta üzerinde sabit konumda durur; Shake() efekti hâlâ çalışır.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GridBoard board;
        [SerializeField] private float height   = 25f;
        [SerializeField] private float tiltDeg  = 70f;

        private Vector3 centerPos;
        private float shakeTimer;
        private float shakeMagnitude;

        private void Start()
        {
            RefreshCenter();
            transform.position = centerPos + Vector3.up * height;
            transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
        }

        public void Shake(float duration = 0.4f, float magnitude = 0.2f)
        {
            shakeTimer     = duration;
            shakeMagnitude = magnitude;
        }

        private void LateUpdate()
        {
            RefreshCenter();
            Vector3 pos = centerPos + Vector3.up * height;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.unscaledDeltaTime;
                pos += (Vector3)(Random.insideUnitCircle * shakeMagnitude);
            }

            transform.position = pos;
            transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
        }

        private void RefreshCenter() =>
            centerPos = board != null ? board.BoardCenter : Vector3.zero;
    }
}
