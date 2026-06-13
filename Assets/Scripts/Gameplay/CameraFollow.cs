using UnityEngine;

namespace BurakOyun.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float lookAheadY = 1f;

        private Vector3 velocity;
        private float shakeTimer;
        private float shakeMagnitude;

        public void Shake(float duration = 0.4f, float magnitude = 0.2f)
        {
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = new Vector3(target.position.x * 0.5f, 0f, target.position.z) + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.unscaledDeltaTime;
                transform.position += (Vector3)(Random.insideUnitCircle * shakeMagnitude);
            }

            transform.LookAt(target.position + Vector3.up * lookAheadY);
        }
    }
}
