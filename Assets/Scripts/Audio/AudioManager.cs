using UnityEngine;
using BurakOyun.Core;

namespace BurakOyun.Audio
{
    /// <summary>
    /// Tüm sesler lokal AudioClip (boşsa SfxGenerator üretir). Yumuşak SFX — korkutucu ses yasak.
    /// Yem → ding, oyun sonu → komik boing, yeni rekor → alkış.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX (yumuşak!)")]
        [SerializeField] private AudioClip dingClip;     // yem yendi
        [SerializeField] private AudioClip boingClip;    // çarpma (komik, korkutucu değil)
        [SerializeField] private AudioClip applauseClip; // yeni rekor

        private void Awake()
        {
            if (dingClip == null) dingClip = SfxGenerator.CreateDing();
            if (boingClip == null) boingClip = SfxGenerator.CreateBoing();
            if (applauseClip == null) applauseClip = SfxGenerator.CreateApplause();
        }

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
            if (score > 0) Play(dingClip); // 0 = oyun başı, ses yok
        }

        private void HandleGameOver(int score, bool newBest)
        {
            Play(boingClip);
            if (newBest) Play(applauseClip);
        }

        private void Play(AudioClip clip)
        {
            if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
        }
    }
}
