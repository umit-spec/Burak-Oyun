using UnityEngine;

namespace BurakOyun.Core
{
    /// <summary>
    /// SADECE lokal kayıt (PlayerPrefs). Hiçbir veri cihaz dışına çıkmaz.
    /// MVP: yalnızca en iyi yıldız sayısı.
    /// </summary>
    public static class SaveManager
    {
        private const string BestStarsKey = "BestStars";

        public static int BestStars => PlayerPrefs.GetInt(BestStarsKey, 0);

        public static void ReportStars(int stars)
        {
            if (stars > BestStars)
            {
                PlayerPrefs.SetInt(BestStarsKey, stars);
                PlayerPrefs.Save();
            }
        }
    }
}
