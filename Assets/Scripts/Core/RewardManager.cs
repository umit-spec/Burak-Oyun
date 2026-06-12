using UnityEngine;
using BurakOyun.Gameplay;

namespace BurakOyun.Core
{
    /// <summary>
    /// Yıldız sayacı + kelime tamam kutlaması (konfeti). Ceza mekanizması YOKTUR.
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        [SerializeField] private WordManager wordManager;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private ParticleSystem collectSparkle;
        [SerializeField] private Transform snake;

        public int Stars { get; private set; }
        public event System.Action<int> OnStarsChanged;

        private void OnEnable()
        {
            wordManager.OnCorrectLetter += HandleCorrect;
            wordManager.OnWordComplete += HandleComplete;
            wordManager.OnWordReset += HandleReset;
        }

        private void OnDisable()
        {
            wordManager.OnCorrectLetter -= HandleCorrect;
            wordManager.OnWordComplete -= HandleComplete;
            wordManager.OnWordReset -= HandleReset;
        }

        private void HandleCorrect(char letter, int index)
        {
            Stars++;
            OnStarsChanged?.Invoke(Stars);
            if (collectSparkle != null)
            {
                collectSparkle.transform.position = snake.position + Vector3.up;
                collectSparkle.Play();
            }
        }

        private void HandleComplete()
        {
            if (confetti != null)
            {
                confetti.transform.position = snake.position + Vector3.up * 2f;
                confetti.Play();
            }
        }

        private void HandleReset()
        {
            Stars = 0;
            OnStarsChanged?.Invoke(Stars);
        }
    }
}
