using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// Bulutu yavaşça sağa kaydırır; sınıra ulaşınca karşı taraftan tekrar çıkar.
    /// Hem X drift hem Y bob → sahne canlı görünür.
    public class CloudDrifter : MonoBehaviour
    {
        [SerializeField] private float driftSpeed = 0.6f;
        [SerializeField] private float driftRange = 18f;
        [SerializeField] private float bobAmplitude = 0.4f;
        [SerializeField] private float bobSpeed = 0.5f;

        private Vector3 origin;
        private float phase;

        private void Start()
        {
            origin = transform.position;
            phase = Random.Range(0f, Mathf.PI * 2f); // her bulut farklı fazda
        }

        private void Update()
        {
            phase += Time.deltaTime;

            float dx = Mathf.Sin(phase * driftSpeed * 0.3f) * driftRange;
            float dy = Mathf.Sin(phase * bobSpeed) * bobAmplitude;

            transform.position = new Vector3(origin.x + dx, origin.y + dy, origin.z);
        }
    }
}
