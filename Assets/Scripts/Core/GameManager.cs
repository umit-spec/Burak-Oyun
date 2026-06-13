using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using BurakOyun.Gameplay;
using BurakOyun.UI;

namespace BurakOyun.Core
{
    /// <summary>
    /// Durum makinesi: Menu → Playing → GameOver → (Tekrar Oyna) → Playing.
    /// Skoru sayar, event yayınlar; UI/ses/ödül bu event'lere abone olur.
    /// Çarpışma "kaybetmek" olarak değil "tekrar dene" olarak sunulur (çocuk dostu).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SnakeController snake;
        [SerializeField] private FoodSpawner foodSpawner;
        [SerializeField] private UIManager ui;
        [SerializeField] private Data.GameConfig config;

        public GameStateManager State { get; } = new();
        public int Score { get; private set; }

        public event Action<int> OnScoreChanged;
        /// <summary>(skor, yeni rekor mu)</summary>
        public event Action<int, bool> OnGameOver;

        public bool IsPaused => State.Current == GameState.Paused;

        [Header("Heceleme Modu (Burak için)")]
        public string currentWord = "UZAY";
        public int currentLetterIndex = 0;
        private int wordListIndex = 0;

        public event Action<string, int> OnWordProgressChanged; // (kelime, hecelenen_harf_sayisi)
        public event Action<string> OnWordCompleted;

        private void OnEnable()
        {
            snake.OnAteFood += HandleAte;
            snake.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            snake.OnAteFood -= HandleAte;
            snake.OnDied -= HandleDied;
        }

        /// OYNA ve TEKRAR OYNA butonlarına bağlanır.
        public void StartGame()
        {
            Score = 0;
            State.SetState(GameState.Playing);

            // Rastgele veya sırayla ilk kelimeyi seç
            if (config != null && config.spellingWords != null && config.spellingWords.Length > 0)
            {
                if (currentLetterIndex >= currentWord.Length || string.IsNullOrEmpty(currentWord))
                {
                    currentWord = config.spellingWords[wordListIndex];
                }
            }
            
            snake.ResetSnake();
            foodSpawner.Clear();
            foodSpawner.SpawnFood();
            snake.IsMoving = true;
            if (ui != null)
            {
                ui.ShowStart(false);
                ui.ShowGameOver(false);
                ui.ShowPause(false);
            }
            OnScoreChanged?.Invoke(Score);
            OnWordProgressChanged?.Invoke(currentWord, currentLetterIndex);
        }

        public void Replay()
        {
            currentLetterIndex = 0;
            StartGame();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.pKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                TogglePause();
        }

        /// Pause butonuna ve P/Escape'e bağlanır.
        public void TogglePause()
        {
            if (State.Current == GameState.Playing) Pause();
            else if (State.Current == GameState.Paused) Resume();
        }

        public void Pause()
        {
            if (State.Current != GameState.Playing) return;
            snake.IsMoving = false; // yılan duraklatıldığında hareket etmez
            State.SetState(GameState.Paused);
            if (ui != null) ui.ShowPause(true);
        }

        public void Resume()
        {
            if (State.Current != GameState.Paused) return;
            snake.IsMoving = true; // kaldığı yerden devam (gövde/skor korunur)
            State.SetState(GameState.Playing);
            if (ui != null) ui.ShowPause(false);
        }

        private void HandleAte()
        {
            Score++;
            OnScoreChanged?.Invoke(Score);
            
            currentLetterIndex++;
            OnWordProgressChanged?.Invoke(currentWord, currentLetterIndex);

            foodSpawner.Clear();

            if (currentLetterIndex >= currentWord.Length)
            {
                OnWordCompleted?.Invoke(currentWord);
                StartCoroutine(CelebrationAndNextWord());
            }
            else
            {
                if (!foodSpawner.SpawnFood())
                    EndGame();
            }
        }

        private IEnumerator CelebrationAndNextWord()
        {
            snake.IsMoving = false;
            // 2 saniye tebrik ekranı/konfeti sürsün
            yield return new WaitForSeconds(2.0f);

            // Bir sonraki kelimeye geç
            if (config != null && config.spellingWords != null && config.spellingWords.Length > 0)
            {
                wordListIndex = (wordListIndex + 1) % config.spellingWords.Length;
                currentWord = config.spellingWords[wordListIndex];
            }
            currentLetterIndex = 0;

            StartGame();
        }

        private void HandleDied()
        {
            // 6.5 yaşındaki Burak için tamamen bağışlayıcı: hemen yanmaz!
            // Geriye seker, uyarı verir ama oyunu bitirmez.
            if (snake != null)
            {
                // Yılanın başını bir adım geriye çekerek sıkışmasını engelleyelim
                snake.PlayHurtFeedback();
            }
            
            if (ui != null)
            {
                // Ekran ortasında tatlı bir uyarı geri bildirimi
                ui.ShowOopsFeedback();
            }

            // IsMoving'i burada AÇMIYORUZ: yılan, ölüm geri bildirimi (HurtRoutine) bitince
            // kendini sürdürür. Böylece "dur → sars → devam et" sırası garanti olur; çarpışma
            // anında aynı tick içinde tekrar çarpıp "Oops" spam'ine yol açmaz. (B-02)
        }

        private void EndGame()
        {
            snake.IsMoving = false;
            foodSpawner.Clear();
            State.SetState(GameState.GameOver);
            bool newBest = SaveManager.ReportScore(Score);
            if (ui != null) ui.ShowGameOverPanel(Score, newBest);
            OnGameOver?.Invoke(Score, newBest);
        }
    }
}
