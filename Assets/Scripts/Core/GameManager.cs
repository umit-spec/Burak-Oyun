using UnityEngine;
using UnityEngine.InputSystem;
using BurakOyun.Gameplay;
using BurakOyun.Audio;
using BurakOyun.UI;

namespace BurakOyun.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SnakeController snake;
        [SerializeField] private SnakeTail snakeTail;
        [SerializeField] private LetterSpawner spawner;
        [SerializeField] private WordManager wordManager;
        [SerializeField] private RewardManager rewardManager;
        [SerializeField] private UIManager ui;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private float completePauseDelay = 3f;

        public enum State { Menu, Playing, Paused, WordComplete, GameOver }
        public State Current { get; private set; } = State.Menu;

        private void OnEnable()
        {
            if (wordManager == null) { Debug.LogError("[GameManager] wordManager atanmamış!"); return; }
            wordManager.OnCorrectLetter += HandleCorrect;
            wordManager.OnWrongLetter   += HandleWrong;
            wordManager.OnWordComplete  += HandleWordComplete;
            if (snake != null) snake.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            if (wordManager == null) return;
            wordManager.OnCorrectLetter -= HandleCorrect;
            wordManager.OnWrongLetter   -= HandleWrong;
            wordManager.OnWordComplete  -= HandleWordComplete;
            if (snake != null) snake.OnDied -= HandleDied;
        }

        public void StartGame()
        {
            if (wordManager == null || snake == null || spawner == null || ui == null)
            {
                Debug.LogError("[GameManager] StartGame: kritik referans eksik — Sahneyi Kur menüsünü yeniden çalıştırın.");
                return;
            }
            CancelInvoke();
            Current = State.Playing;
            Time.timeScale = 1f;
            wordManager.ResetWord();
            if (snakeTail != null) snakeTail.ClearSegments();
            snake.ResetToStart();
            snake.IsMoving = false; // HAZIR? gecikmesi sonrası başlar
            spawner.StartSpawning();
            ui.ShowStart(false);
            ui.ShowComplete(false);
            ui.ShowPause(false);
            ui.ShowGameOver(false);
            ui.UpdateHighScore(SaveManager.BestStars);
            ui.ShowHint("HAZIR?");
            Invoke(nameof(BeginMoving), 1.2f);
        }

        private void BeginMoving()
        {
            if (Current == State.Playing) snake.IsMoving = true;
        }

        public void Replay() => StartGame();

        public void TogglePause()
        {
            if (Current == State.Playing)
            {
                Current = State.Paused;
                Time.timeScale = 0f;
                snake.IsMoving = false;
                ui.ShowPause(true);
            }
            else if (Current == State.Paused)
            {
                Current = State.Playing;
                Time.timeScale = 1f;
                snake.IsMoving = true;
                ui.ShowPause(false);
            }
        }

        public void ToggleSound()
        {
            if (audioManager != null) audioManager.ToggleSound();
            ui.UpdateSoundButton(audioManager == null || !audioManager.IsMuted);
        }

        private void HandleCorrect(char letter, int index)
        {
            snake.Grow();
            snake.AddSpeedBoost();
        }

        private void HandleWrong(char letter) => snake.ApplySlowdown();

        private void HandleWordComplete()
        {
            Current = State.WordComplete;
            spawner.StopAndClear();
            SaveManager.ReportStars(rewardManager.Stars);
            ui.UpdateHighScore(SaveManager.BestStars);
            if (cameraFollow != null) cameraFollow.Shake(0.5f, 0.2f);
            Invoke(nameof(StopSnake), completePauseDelay);
        }

        private void HandleDied()
        {
            if (Current != State.Playing) return;
            Current = State.GameOver;
            CancelInvoke(nameof(BeginMoving));
            spawner.StopAndClear();
            if (audioManager != null) audioManager.PlayDeath();
            ui.TriggerDeathFlash();
            if (cameraFollow != null) cameraFollow.Shake(0.35f, 0.18f);
            Invoke(nameof(ShowGameOverPanel), 0.6f); // flash bittikten sonra panel çık
        }

        private void ShowGameOverPanel() => ui.ShowGameOver(true);

        private void StopSnake() => snake.IsMoving = false;

        private void Update() => HandleBackButton();

        private void HandleBackButton()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            switch (Current)
            {
                case State.Playing:
                    TogglePause();
                    break;
                case State.Paused:
                    TogglePause();
                    break;
                case State.WordComplete:
                    CancelInvoke(nameof(StopSnake));
                    ReturnToMenu();
                    break;
                case State.GameOver:
                    ReturnToMenu();
                    break;
                case State.Menu:
                    Application.Quit();
                    break;
            }
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            Current = State.Menu;
            snake.IsMoving = false;
            spawner.StopAndClear();
            ui.ShowStart(true);
            ui.ShowComplete(false);
            ui.ShowPause(false);
            ui.ShowGameOver(false);
        }
    }
}
