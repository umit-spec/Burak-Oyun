using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BurakOyun.Gameplay;
using BurakOyun.Core;

namespace BurakOyun.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private WordManager wordManager;
        [SerializeField] private RewardManager rewardManager;

        [Header("HUD")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text highScoreText;   // "En İyi: ★ N"
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private TMP_Text soundButtonLabel; // "🔊" / "🔇"

        [Header("Paneller")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private GameObject pausePanel;

        [Header("Canvas")]
        [SerializeField] private Canvas mainCanvas; // floating text parent

        [Header("Renkler")]
        [SerializeField] private Color collectedColor = new Color(1f, 0.72f, 0.1f);
        [SerializeField] private Color pendingColor   = new Color(0.75f, 0.78f, 0.82f);
        [SerializeField] private float feedbackDuration = 1.5f;

        private float feedbackTimer;
        private float feedbackScale;

        private void OnEnable()
        {
            if (wordManager == null) { Debug.LogError("[UIManager] wordManager atanmamış!"); return; }
            wordManager.OnCorrectLetter  += HandleCorrect;
            wordManager.OnWrongLetter    += HandleWrong;
            wordManager.OnWordComplete   += HandleComplete;
            wordManager.OnWordReset      += RefreshProgress;
            if (rewardManager != null) rewardManager.OnStarsChanged += HandleStars;
        }

        private void OnDisable()
        {
            if (wordManager == null) return;
            wordManager.OnCorrectLetter  -= HandleCorrect;
            wordManager.OnWrongLetter    -= HandleWrong;
            wordManager.OnWordComplete   -= HandleComplete;
            wordManager.OnWordReset      -= RefreshProgress;
            if (rewardManager != null) rewardManager.OnStarsChanged -= HandleStars;
        }

        private void Start()
        {
            RefreshProgress();
            ShowStart(true);
            ShowComplete(false);
            ShowPause(false);
            if (feedbackText != null) feedbackText.text = "";
            UpdateHighScore(SaveManager.BestStars);
        }

        private void Update()
        {
            if (feedbackTimer > 0f)
            {
                feedbackTimer -= Time.deltaTime;
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

        public void ShowStart(bool show)    { if (startPanel    != null) startPanel.SetActive(show); }
        public void ShowComplete(bool show) { if (completePanel != null) completePanel.SetActive(show); }
        public void ShowPause(bool show)    { if (pausePanel    != null) pausePanel.SetActive(show); }

        public void UpdateHighScore(int best)
        {
            if (highScoreText != null) highScoreText.text = "En İyi: ★ " + best;
        }

        public void UpdateSoundButton(bool soundOn)
        {
            if (soundButtonLabel != null) soundButtonLabel.text = soundOn ? "♪" : "✕";
        }

        private void HandleCorrect(char letter, int index)
        {
            RefreshProgress();
            ShowFeedback("Aferin!");
            SpawnFloatingText("+1 ★", new Color(1f, 0.85f, 0.1f));
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

        private void SpawnFloatingText(string text, Color color)
        {
            if (mainCanvas == null) return;
            var go = new GameObject("FloatingTxt");
            go.transform.SetParent(mainCanvas.transform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 52;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = tmp.rectTransform;
            rt.sizeDelta = new Vector2(200f, 80f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.45f);
            rt.anchoredPosition = Vector2.zero;
            StartCoroutine(AnimateFloat(rt, tmp));
        }

        private IEnumerator AnimateFloat(RectTransform rt, TextMeshProUGUI tmp)
        {
            float duration = 1.1f;
            float elapsed = 0f;
            Vector2 start = rt.anchoredPosition;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = start + Vector2.up * (120f * t);
                tmp.alpha = 1f - t;
                yield return null;
            }
            Destroy(rt.gameObject);
        }
    }
}
