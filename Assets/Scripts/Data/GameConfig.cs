using UnityEngine;

namespace BurakOyun.Data
{
    /// <summary>
    /// Tüm oynanış ayarları tek yerden (designer-friendly).
    /// Ev testinden sonra hız/ızgara buradan ayarlanır, kod değişmez.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BurakOyun/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Izgara")]
        [Tooltip("Izgara genişliği (hücre).")]
        public int gridWidth = 20;
        [Tooltip("Izgara yüksekliği (hücre).")]
        public int gridHeight = 15;
        [Tooltip("Bir hücrenin dünya boyutu (m).")]
        public float cellSize = 1f;

        [Header("Yılan")]
        [Tooltip("Hücre başına saniye. Çocuk dostu yavaş tempo: 0.25-0.35.")]
        public float tickRate = 0.3f;
        [Tooltip("Başlangıç uzunluğu (baş dahil).")]
        public int initialLength = 3;

        [Header("Zorluk (çocuk dostu — yumuşak hızlanma)")]
        [Tooltip("Her yemekte tick bu kadar kısalır (0 = hiç hızlanmaz).")]
        public float speedUpPerFood = 0.003f;
        [Tooltip("Tick süresi taban sınırı — oyun asla bundan hızlı olmaz.")]
        public float minTickRate = 0.18f;

        [Header("Uzay Heceleme Modu (Burak için)")]
        [Tooltip("Burak'ın heceleyeceği Türkçe kelimelerin listesi.")]
        public string[] spellingWords = new string[] { "BURAK", "UZAY", "ANNE", "BABA", "DUNYA", "MARS", "YILDIZ", "ROKET" };

        [Tooltip("Yılanın uzay mekiği kafasının rengi.")]
        public Color spaceshipHeadColor = new Color(0.95f, 0.25f, 0.25f);

        [Tooltip("Gezegen gövde segmentleri için renk havuzu.")]
        public Color[] planetSegmentColors = new Color[]
        {
            new Color(0.98f, 0.65f, 0.12f), // Satürn sarısı
            new Color(0.22f, 0.62f, 0.95f), // Neptün mavisi
            new Color(0.95f, 0.32f, 0.32f), // Mars kızılı
            new Color(0.38f, 0.85f, 0.38f), // Uranüs yeşili
            new Color(0.75f, 0.45f, 0.95f)  // Kozmik mor
        };
    }
}
