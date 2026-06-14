using UnityEngine;

namespace BurakOyun.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BurakOyun/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Izgara")]
        [Tooltip("Kaç sütun (x).")]
        public int gridWidth = 15;
        [Tooltip("Kaç satır (z).")]
        public int gridHeight = 15;
        [Tooltip("Her hücrenin kenar uzunluğu (m).")]
        public float cellSize = 1.5f;

        [Header("Yılan Hareketi")]
        [Tooltip("Her tick arasındaki süre (sn). Küçük = daha hızlı.")]
        public float tickInterval = 0.35f;
        [Tooltip("Hız artışı sırasında minimum tick aralığı (sn).")]
        public float minTickInterval = 0.12f;

        [Header("Harf Spawn")]
        [Range(1, 2)] public int decoyCount = 2;
        [Tooltip("Çeldirici havuzu (Türk alfabesi, büyük harf).")]
        public string decoyAlphabet = "ABCÇDEFGHIİJKLMNOÖPRSŞTUÜVYZ";
    }
}
