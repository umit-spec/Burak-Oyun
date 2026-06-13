using UnityEngine;

namespace BurakOyun.Core
{
    /// <summary>
    /// SADECE lokal kayıt (PlayerPrefs). Hiçbir veri cihaz dışına çıkmaz.
    /// MVP: yalnızca en iyi skor.
    /// </summary>
    public static class SaveManager
    {
        private const string BestScoreKey = "BestScore";

        public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

        /// <summary>Skoru bildirir; yeni rekorsa kaydeder ve true döner.</summary>
        public static bool ReportScore(int score)
        {
            if (score <= BestScore) return false;
            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}
