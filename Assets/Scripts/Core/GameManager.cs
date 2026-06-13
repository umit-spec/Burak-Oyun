using UnityEngine;
using UnityEngine.InputSystem;
using BurakOyun.Gameplay;
using BurakOyun.UI;

namespace BurakOyun.Core
{
    /// <summary>
    /// Durum makinesi: Menu → Playing → WordComplete → (Tekrar Oyna) → Playing.
    /// "Game Over" durumu bilinçli olarak YOKTUR.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SnakeController snake;
        [SerializeField] private LetterSpawner spawner;
        [SerializeField] private WordManager wordManager;
        [SerializeField] private RewardManager rewardManager;
        [SerializeField] private UIManager ui;
        [SerializeField] private float completePauseDelay = 3f; // kutlamayı izleme süresi

        public enum State { Menu, Playing, WordComplete }
        public State Current { get; private set; } = State.Menu;

        private void OnEnable()
        {
            wordManager.OnWrongLetter += HandleWrong;
            wordManager.OnWordComplete += HandleWordComplete;
        }

        private void OnDisable()
        {
            wordManager.OnWrongLetter -= HandleWrong;
            wordManager.OnWordComplete -= HandleWordComplete;
        }

        /// OYNA butonuna bağlanır.
        public void StartGame()
        {
            Current = State.Playing;
            wordManager.ResetWord();
            snake.ResetToStart();
            snake.IsMoving = true;
            spawner.StartSpawning();
            ui.ShowStart(false);
            ui.ShowComplete(false);
        }

        /// TEKRAR OYNA butonuna bağlanır.
        public void Replay() => StartGame();

        private void HandleWrong(char letter) => snake.ApplySlowdown();

        private void HandleWordComplete()
        {
            Current = State.WordComplete;
            spawner.StopAndClear();
            SaveManager.ReportStars(rewardManager.Stars);
            Invoke(nameof(StopSnake), completePauseDelay);
        }

        private void StopSnake() => snake.IsMoving = false;

        private void Update()
        {
            HandleBackButton();
        }

        private void HandleBackButton()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            switch (Current)
            {
                case State.Playing:
                case State.WordComplete:
                    CancelInvoke(nameof(StopSnake));
                    Current = State.Menu;
                    snake.IsMoving = false;
                    spawner.StopAndClear();
                    ui.ShowStart(true);
                    ui.ShowComplete(false);
                    break;
                case State.Menu:
                    Application.Quit();
                    break;
            }
        }
    }
}
