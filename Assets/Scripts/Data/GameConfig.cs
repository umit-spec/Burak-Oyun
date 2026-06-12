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
    }
}
