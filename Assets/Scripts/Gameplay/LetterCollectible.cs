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

        public void Init(char letter, LetterSpawner spawner, bool target = false)
        {
            Letter    = letter;
            owner     = spawner;
            collected = false;
            isTarget  = target;
            bobPhase  = Random.Range(0f, Mathf.PI * 2f);

            if (label != null) label.text = letter.ToString();

            var glow = GetComponentInChildren<LetterGlow>(true);
            if (glow != null) glow.SetColor(LetterGlow.LetterColor(letter));

            // Hedef harf biraz daha büyük başlar
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
            transform.Rotate(0f, 55f * Time.deltaTime, 0f);

            bobPhase += Time.deltaTime * 2.2f;
            float bob = Mathf.Sin(bobPhase) * 0.1f;
            var pos = transform.position;
            pos.y = 0.85f + bob;
            transform.position = pos;

            if (isTarget)
            {
                // Hedef harf nabız gibi büyüyüp küçülür
                float pulse = 1.18f + 0.06f * Mathf.Sin(Time.time * 3.5f);
                transform.localScale = Vector3.one * pulse;
            }
        }
    }
}
