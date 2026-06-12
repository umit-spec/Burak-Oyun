using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Yılanın arkasından yumuşak takip. Sarsıntı/ani hareket yok (çocuk dostu).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float lookAheadY = 1f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            // X'te şerit geçişini biraz takip et ama tam kilitlenme (yumuşaklık için yarısı)
            Vector3 desired = new Vector3(target.position.x * 0.5f, 0f, target.position.z) + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * lookAheadY);
        }
    }
}
