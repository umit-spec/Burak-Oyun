using UnityEngine;

namespace BurakOyun.Data
{
    /// <summary>
    /// Tüm oynanış ayarları tek yerden (designer-friendly).
    /// Ev testinden sonra hız/aralık buradan ayarlanır, kod değişmez.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BurakOyun/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Yılan Hareketi")]
        [Tooltip("İleri hız (m/sn). Çocuk dostu yavaş tempo: 3-5 arası.")]
        public float forwardSpeed = 4f;
        [Tooltip("Şeritler arası mesafe (m).")]
        public float laneWidth = 2.5f;
        [Tooltip("Şerit değiştirme yumuşaklığı (büyük = hızlı geçiş).")]
        public float laneChangeSpeed = 8f;

        [Header("Yanlış Harf (yumuşak geri bildirim, ceza değil)")]
        [Range(0.1f, 1f)] public float slowdownFactor = 0.7f;
        public float slowdownDuration = 2f;

        [Header("Harf Spawn")]
        [Tooltip("Dalgalar arası süre (sn). ~10 sn → 1 dk'da kelime biter.")]
        public float spawnInterval = 9f;
        [Tooltip("Yanlış (çeldirici) harf sayısı: 1 veya 2.")]
        [Range(1, 2)] public int decoyCount = 2;
        [Tooltip("Harflerin yılanın ne kadar önünde belireceği (m).")]
        public float spawnDistance = 45f;
        [Tooltip("Yılanın arkasında kalınca despawn mesafesi (m).")]
        public float despawnBehind = 5f;
        [Tooltip("Çeldirici havuzu (Türk alfabesi, büyük harf).")]
        public string decoyAlphabet = "ABCÇDEFGHIİJKLMNOÖPRSŞTUÜVYZ";
    }
}
