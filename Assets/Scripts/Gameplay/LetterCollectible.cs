using UnityEngine;
using TMPro;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Yoldaki tek bir harf. Yılan değince WordManager'a bildirir.
    /// Havuzdan (pool) yönetilir; Destroy edilmez, deaktif edilir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LetterCollectible : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        public char Letter { get; private set; }

        private LetterSpawner owner;
        private bool collected;

        public void Init(char letter, LetterSpawner spawner)
        {
            Letter = letter;
            owner = spawner;
            collected = false;
            if (label != null) label.text = letter.ToString();
            var glow = GetComponentInChildren<LetterGlow>(true);
            if (glow != null) glow.SetColor(LetterGlow.LetterColor(letter));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || owner == null || !other.CompareTag("Player")) return;
            collected = true;
            owner.NotifyCollected(this);
        }

        private void Update()
        {
            transform.Rotate(0f, 60f * Time.deltaTime, 0f);
            // Hafif süzülme
            float bob = Mathf.Sin(Time.time * 2f + transform.position.x) * 0.1f;
            var pos = transform.position;
            pos.y = 1f + bob;
            transform.position = pos;
        }
    }
}
