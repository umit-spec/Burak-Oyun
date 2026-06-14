using UnityEngine;
using TMPro;

namespace BurakOyun.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class LetterCollectible : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public char Letter { get; private set; }

        private LetterSpawner owner;
        private bool collected;
        private bool isTarget;
        private float bobPhase;
        private float spawnY;  // letterHeight'i Update'in üzerine yazmasını önler (K-3)

        public void Init(char letter, LetterSpawner spawner, bool target = false)
        {
            Letter    = letter;
            owner     = spawner;
            collected = false;
            isTarget  = target;
            bobPhase  = Random.Range(0f, Mathf.PI * 2f);
            spawnY    = transform.position.y; // pool'dan her çıkışta taze Y

            if (label != null) label.text = letter.ToString();

            var glow = GetComponentInChildren<LetterGlow>(true);
            if (glow != null) glow.SetColor(LetterGlow.LetterColor(letter));

            transform.localScale = target ? Vector3.one * 1.18f : Vector3.one;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || owner == null || !other.CompareTag("Player")) return;
            collected = true;
            owner.NotifyCollected(this);
        }

        private void Update()
        {
            // Dönüş animasyonu
            transform.Rotate(0f, 55f * Time.deltaTime, 0f);

            // Hedef harf nabzı (P-3: görsel-only işlemler LateUpdate'e taşındı ancak
            // scale animasyonu rotasyonla aynı frekansta tutmak için Update'te kaldı)
            if (isTarget)
            {
                float pulse = 1.18f + 0.06f * Mathf.Sin(Time.time * 3.5f);
                transform.localScale = Vector3.one * pulse;
            }
        }

        private void LateUpdate()
        {
            // Bob animasyonu LateUpdate'te (P-3): fizik/rendering son aşamada
            bobPhase += Time.deltaTime * 2.2f;
            var pos = transform.position;
            pos.y = spawnY + Mathf.Sin(bobPhase) * 0.1f; // spawnY kullan, 1f sabitini değil (K-3)
            transform.position = pos;
        }
    }
}
