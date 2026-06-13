using UnityEngine;
using BurakOyun.Gameplay;

namespace BurakOyun.Core
{
    /// <summary>
    /// Yem yenince parıltı, yeni rekorla biten oyunda konfeti. Ceza efekti YOKTUR.
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SnakeController snake;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private ParticleSystem collectSparkle;

        private void OnEnable()
        {
            gameManager.OnScoreChanged += HandleScore;
            gameManager.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            gameManager.OnScoreChanged -= HandleScore;
            gameManager.OnGameOver -= HandleGameOver;
        }

        private void HandleScore(int score)
        {
            if (score <= 0 || collectSparkle == null) return; // 0 = oyun başı, yem yenmedi
            collectSparkle.transform.position = snake.HeadWorldPosition + Vector3.up;
            collectSparkle.Play();
        }

        private void HandleGameOver(int score, bool newBest)
        {
            if (snake != null) snake.PlayHurtFeedback(); // her ölümde kısa baş flaş + sarsıntı
            if (!newBest || confetti == null) return;
            confetti.transform.position = snake.HeadWorldPosition + Vector3.up * 2f;
            confetti.Play();
        }
    }
}
