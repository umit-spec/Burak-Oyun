using UnityEngine;
using TMPro;
using BurakOyun.Gameplay;
using BurakOyun.Core;

namespace BurakOyun.UI
{
    /// <summary>
    /// İlerleme göstergesi (B U R A K), yıldız sayacı, feedback yazıları.
    /// Yanlışta asla kırmızı X — gülen yüz + "Tekrar dene!".
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private WordManager wordManager;
        [SerializeField] private RewardManager rewardManager;

        [Header("UI Referansları")]
        [SerializeField] private TMP_Text progressText;   // "B U R A K" (toplananlar renkli)
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private TMP_Text feedbackText;   // "Harika!" / "Tekrar dene! 🙂"
        [SerializeField] private GameObject startPanel;   // büyük OYNA butonu
        [SerializeField] private GameObject completePanel;// "Harika Burak!" + Tekrar Oyna

        [Header("Renkler")]
        [SerializeField] private Color collectedColor = new Color(1f, 0.72f, 0.1f); // sıcak sarı
        [SerializeField] private Color pendingColor = new Color(0.75f, 0.78f, 0.82f); // soluk gri

        [SerializeField] private float feedbackDuration = 1.5f;
        private float feedbackTimer;

        private void OnEnable()
        {
            wordManager.OnCorrectLetter += HandleCorrect;
            wordManager.OnWrongLetter += HandleWrong;
            wordManager.OnWordComplete += HandleComplete;
            wordManager.OnWordReset += RefreshProgress;
            rewardManager.OnStarsChanged += HandleStars;
        }

        private void OnDisable()
        {
            wordManager.OnCorrectLetter -= HandleCorrect;
            wordManager.OnWrongLetter -= HandleWrong;
            wordManager.OnWordComplete -= HandleComplete;
            wordManager.OnWordReset -= RefreshProgress;
            rewardManager.OnStarsChanged -= HandleStars;
        }

        private void Start()
        {
            RefreshProgress();
            ShowStart(true);
            ShowComplete(false);
            if (feedbackText != null) feedbackText.text = "";
        }

        private float feedbackScale;

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
                // Zıplayan feedback animasyonu
                if (feedbackText != null)
                {
                    feedbackScale = Mathf.Lerp(feedbackScale, 1f, Time.deltaTime * 8f);
                    feedbackText.transform.localScale = Vector3.one * feedbackScale;
                }
                if (feedbackTimer <= 0f && feedbackText != null)
                {
                    feedbackText.text = "";
                    feedbackText.transform.localScale = Vector3.one;
                }
            }
        }

        public void ShowStart(bool show) { if (startPanel != null) startPanel.SetActive(show); }
        public void ShowComplete(bool show) { if (completePanel != null) completePanel.SetActive(show); }

        private void HandleCorrect(char letter, int index)
        {
            RefreshProgress();
            ShowFeedback("Aferin!");
        }

        private void HandleWrong(char letter) => ShowFeedback("Tekrar dene! :)");

        private void HandleComplete()
        {
            ShowFeedback("HARİKA!");
            ShowComplete(true);
        }

        private void HandleStars(int stars)
        {
            if (starsText != null) starsText.text = "★ " + stars;
        }

        private void ShowFeedback(string msg)
        {
            if (feedbackText == null) return;
            feedbackText.text = msg;
            feedbackTimer = feedbackDuration;
            feedbackScale = 1.4f;
        }

        private void RefreshProgress()
        {
            if (progressText == null) return;
            string word = wordManager.Word;
            int idx = wordManager.CurrentIndex;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < word.Length; i++)
            {
                Color c = i < idx ? collectedColor : pendingColor;
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{word[i]}</color> ");
            }
            progressText.text = sb.ToString().TrimEnd();
        }
    }
}
