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

        [SerializeField] private float feedbackDuration = 1.2f;
        private float feedbackTimer;
        private float feedbackScale;

        private void OnEnable()
        {
            gameManager.OnScoreChanged += HandleScore;
        }

        private void OnDisable()
        {
            gameManager.OnScoreChanged -= HandleScore;
        }

        private void Start()
        {
            ShowStart(true);
            ShowGameOver(false);
            RefreshScore(0);
            RefreshBest();
            if (feedbackText != null) feedbackText.text = "";
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

        private void HandleScore(int score)
        {
            RefreshScore(score);
            if (score > 0) // 0 = oyun başı
                ShowFeedback(score % 5 == 0 ? "SÜPER!" : "Aferin!");
        }

        private void RefreshScore(int score)
        {
            if (scoreText != null) scoreText.text = "Skor: " + score;
        }

        private void RefreshBest()
        {
            if (bestText != null) bestText.text = "En İyi: " + SaveManager.BestScore;
        }

        private void ShowFeedback(string msg)
        {
            if (feedbackText == null) return;
            feedbackText.text = msg;
            feedbackTimer = feedbackDuration;
            feedbackScale = 1.4f;
        }
    }
}
