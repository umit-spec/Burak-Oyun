using UnityEngine;
using TMPro;
using BurakOyun.Core;

namespace BurakOyun.UI
{
    /// <summary>
    /// Skor göstergesi, başlangıç ve oyun sonu panelleri.
    /// Oyun sonunda asla "KAYBETTİN" yok — skor + "TEKRAR OYNA" (çocuk dostu).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Header("UI Referansları")]
        [SerializeField] private TMP_Text scoreText;        // "Skor: 0"
        [SerializeField] private TMP_Text bestText;         // "En İyi: 0"
        [SerializeField] private TMP_Text feedbackText;     // "Aferin!"
        [SerializeField] private GameObject startPanel;     // büyük OYNA butonu
        [SerializeField] private GameObject gameOverPanel;  // skor + TEKRAR OYNA
        [SerializeField] private TMP_Text gameOverScoreText;
        [SerializeField] private GameObject pausePanel;     // DURAKLATILDI + DEVAM ET

        [SerializeField] private float feedbackDuration = 1.2f;
        private float feedbackTimer;
        private float feedbackScale;

        [Header("Heceleme Modu UI")]
        [SerializeField] private TMP_Text spellingText;     // kelime ilerleyişi örn: "U Z A [Y]"

        private static readonly Color PositiveColor = Color.white;
        private static readonly Color OopsColor = new Color(0.97f, 0.32f, 0.32f);   // kırmızı uyarı
        private static readonly Color CelebrateColor = new Color(1f, 0.84f, 0.1f);  // altın kutlama

        private void OnEnable()
        {
            gameManager.OnScoreChanged += HandleScore;
            gameManager.OnWordProgressChanged += HandleWordProgress;
            gameManager.OnWordCompleted += HandleWordCompleted;
        }

        private void OnDisable()
        {
            gameManager.OnScoreChanged -= HandleScore;
            gameManager.OnWordProgressChanged -= HandleWordProgress;
            gameManager.OnWordCompleted -= HandleWordCompleted;
        }

        private void Start()
        {
            ShowStart(true);
            ShowGameOver(false);
            ShowPause(false);
            RefreshScore(0);
            RefreshBest();
            if (feedbackText != null) feedbackText.text = "";
            if (spellingText != null) spellingText.text = "";
        }

        private void Update()
        {
            if (feedbackTimer <= 0f) return;
            feedbackTimer -= Time.deltaTime;
            // Zıplayan feedback animasyonu
            if (feedbackText != null)
            {
                feedbackScale = Mathf.Lerp(feedbackScale, 1f, Time.deltaTime * 8f);
                feedbackText.transform.localScale = Vector3.one * feedbackScale;
                if (feedbackTimer <= 0f)
                {
                    feedbackText.text = "";
                    feedbackText.transform.localScale = Vector3.one;
                }
            }
        }

        public void ShowStart(bool show)
        {
            if (startPanel != null) startPanel.SetActive(show);
        }

        public void ShowGameOver(bool show)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(show);
        }

        public void ShowPause(bool show)
        {
            if (pausePanel != null) pausePanel.SetActive(show);
        }

        /// <summary>Oyun sonu panelini skorla doldurup gösterir.</summary>
        public void ShowGameOverPanel(int score, bool newBest)
        {
            if (gameOverScoreText != null)
                gameOverScoreText.text = newBest
                    ? $"YENİ REKOR!\nSkor: {score}"
                    : $"Skor: {score}";
            RefreshBest();
            ShowGameOver(true);
            ShowFeedback(newBest ? "HARİKA!" : "İyi deneme! :)");
        }

        public void ShowOopsFeedback()
        {
            // Kırmızı renk + ekstra büyük zıplama: 6 yaş için fark edilir görsel uyarı (B-09).
            ShowFeedback("Oops! Baska Yone!", OopsColor);
            feedbackScale = 1.9f;
        }

        private void HandleWordCompleted(string word)
        {
            // Kelime tamamlandı: Burak için büyük, neşeli altın kutlama (B-04).
            ShowFeedback($"AFERIN!  {word} TAMAM!", CelebrateColor);
        }

        private void HandleWordProgress(string word, int letterIndex)
        {
            if (spellingText == null) return;
            
            // Burak için harfleri okunaklı şekilde aralara tire (-) koyarak gösterelim.
            // Henüz toplanmamış sıradaki harfi parantez içine [ ] alarak vurgulayalım.
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < word.Length; i++)
            {
                if (i > 0) sb.Append(" - ");
                if (i < letterIndex)
                {
                    sb.Append($"<color=#FFD700>{word[i]}</color>"); // toplanmış harfler altın sarısı
                }
                else if (i == letterIndex)
                {
                    sb.Append($"<b><color=#00FFFF>[{word[i]}]</color></b>"); // sıradaki harf parlayan turkuaz ve kalın
                }
                else
                {
                    sb.Append($"<color=#778899>{word[i]}</color>"); // gelecek harfler gri
                }
            }
            spellingText.text = sb.ToString();
        }

        private void HandleScore(int score)
        {
            RefreshScore(score);
            if (score > 0) // 0 = oyun başı
                ShowFeedback(score % 5 == 0 ? "HARIKA!" : "Aferin!");
        }

        private void RefreshScore(int score)
        {
            if (scoreText != null) scoreText.text = "Skor: " + score;
        }

        private void RefreshBest()
        {
            if (bestText != null) bestText.text = "En İyi: " + SaveManager.BestScore;
        }

        private void ShowFeedback(string msg, Color? color = null)
        {
            if (feedbackText == null) return;
            feedbackText.text = msg;
            feedbackText.color = color ?? PositiveColor; // her seferinde deterministik renk
            feedbackTimer = feedbackDuration;
            feedbackScale = 1.4f;
        }
    }
}
