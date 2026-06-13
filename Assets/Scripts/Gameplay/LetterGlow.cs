using UnityEngine;

namespace BurakOyun.Gameplay
{
    public class LetterGlow : MonoBehaviour
    {
        [SerializeField] private Color glowColor = new Color(0.3f, 0.7f, 1f);

        private Renderer rend;
        private float phase;
        private Vector3 baseScale;
        private Material mat;

        private void Start()
        {
            baseScale = transform.localScale;
            rend = GetComponent<Renderer>();
            mat = new Material(rend.sharedMaterial);
            rend.material = mat;
        }

        private void Update()
        {
            phase += Time.deltaTime * 2.5f;

            float scalePulse = Mathf.Sin(phase) * 0.04f;
            transform.localScale = baseScale * (1f + scalePulse);

            float brightnessPulse = Mathf.Sin(phase * 1.5f) * 0.15f;
            Color pulsedColor = new Color(
                Mathf.Clamp01(glowColor.r + brightnessPulse),
                Mathf.Clamp01(glowColor.g + brightnessPulse),
                Mathf.Clamp01(glowColor.b + brightnessPulse),
                glowColor.a
            );
            mat.color = pulsedColor;
        }

        public void SetColor(Color c)
        {
            glowColor = c;
            if (mat != null)
                mat.color = c;
        }

        public static Color LetterColor(char c)
        {
            switch (c)
            {
                case 'B': return new Color(1f, 0.35f, 0.35f);
                case 'U': return new Color(1f, 0.65f, 0.2f);
                case 'R': return new Color(1f, 0.95f, 0.2f);
                case 'A': return new Color(0.25f, 0.9f, 0.4f);
                case 'K': return new Color(0.3f, 0.6f, 1f);
                default:  return new Color(0.8f, 0.4f, 1f);
            }
        }
    }
}
