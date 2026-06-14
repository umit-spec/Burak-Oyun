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

        private void OnDestroy()
        {
            if (mat != null) Destroy(mat);
        }

        public void SetColor(Color c)
        {
            glowColor = c;
            if (mat != null)
                mat.color = c;
        }

        public static Color LetterColor(char c) => c switch
        {
            // Sesli harfler — sıcak tonlar
            'A' or 'a' => new Color(0.25f, 0.90f, 0.40f), // yeşil
            'E' or 'e' => new Color(0.30f, 0.85f, 0.70f), // turkuaz-yeşil
            'I' or 'ı' => new Color(0.40f, 0.75f, 1.00f), // açık mavi
            'İ' or 'i' => new Color(0.40f, 0.75f, 1.00f), // açık mavi
            'O' or 'o' => new Color(1.00f, 0.55f, 0.15f), // turuncu
            'Ö' or 'ö' => new Color(1.00f, 0.45f, 0.70f), // pembe
            'U' or 'u' => new Color(1.00f, 0.65f, 0.20f), // amber
            'Ü' or 'ü' => new Color(0.90f, 0.50f, 1.00f), // leylak

            // Sessiz harfler — soğuk/canlı tonlar
            'B' or 'b' => new Color(1.00f, 0.35f, 0.35f), // kırmızı-mercan
            'C' or 'c' => new Color(0.40f, 0.90f, 0.55f), // nane yeşil
            'Ç' or 'ç' => new Color(0.20f, 0.80f, 0.60f), // zümrüt
            'D' or 'd' => new Color(0.55f, 0.40f, 1.00f), // mor
            'F' or 'f' => new Color(1.00f, 0.80f, 0.20f), // altın
            'G' or 'g' => new Color(0.50f, 1.00f, 0.35f), // limon yeşil
            'Ğ' or 'ğ' => new Color(0.35f, 0.90f, 0.50f), // açık yeşil
            'H' or 'h' => new Color(1.00f, 0.60f, 0.60f), // salmon
            'J' or 'j' => new Color(0.80f, 0.30f, 1.00f), // eflatun
            'K' or 'k' => new Color(0.30f, 0.60f, 1.00f), // mavi
            'L' or 'l' => new Color(0.20f, 0.75f, 1.00f), // gökyüzü mavisi
            'M' or 'm' => new Color(1.00f, 0.40f, 0.70f), // fuşya
            'N' or 'n' => new Color(0.60f, 0.90f, 0.30f), // fıstık yeşil
            'P' or 'p' => new Color(1.00f, 0.70f, 0.30f), // şeftali
            'R' or 'r' => new Color(1.00f, 0.95f, 0.20f), // sarı
            'S' or 's' => new Color(0.30f, 0.85f, 0.85f), // cyan
            'Ş' or 'ş' => new Color(0.20f, 0.70f, 0.90f), // koyu cyan
            'T' or 't' => new Color(0.75f, 0.50f, 1.00f), // lavanta
            'V' or 'v' => new Color(1.00f, 0.45f, 0.25f), // ateş kırmızısı
            'Y' or 'y' => new Color(0.50f, 1.00f, 0.60f), // açık yeşil
            'Z' or 'z' => new Color(0.85f, 0.25f, 0.60f), // çilek
            _           => new Color(0.80f, 0.80f, 0.85f), // gümüş (bilinmeyen)
        };
    }
}
