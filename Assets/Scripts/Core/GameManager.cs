using System;
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

        public GameStateManager State { get; } = new();
        public int Score { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action OnGameStarted;
        /// <summary>(skor, yeni rekor mu)</summary>
        public event Action<int, bool> OnGameOver;

        public bool IsPaused => State.Current == GameState.Paused;

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
            OnGameStarted?.Invoke();
        }

        public void Replay() => StartGame();

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
            foodSpawner.Clear();
            if (!foodSpawner.SpawnFood())
                EndGame(); // tahta tamamen doldu — bu bir zafer, yine de kutlanır :)
        }

        private void HandleDied() => EndGame();

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
