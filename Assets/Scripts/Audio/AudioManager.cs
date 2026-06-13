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
        [SerializeField] private AudioClip musicClip;    // sakin loop (boşsa üretilir)

        [Tooltip("Müzik ses düzeyi — çok düşük, rahatsız etmesin.")]
        [SerializeField] private float musicVolume = 0.12f;

        private void Awake()
        {
            if (dingClip == null) dingClip = SfxGenerator.CreateDing();
            if (boingClip == null) boingClip = SfxGenerator.CreateBoing();
            if (applauseClip == null) applauseClip = SfxGenerator.CreateApplause();
            if (musicClip == null) musicClip = SfxGenerator.CreateMusicLoop();
        }

        private void Start()
        {
            if (musicSource == null) return;
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.volume = musicVolume; // çok düşük
            musicSource.Play();
        }

        private void OnEnable()
        {
            gameManager.OnScoreChanged += HandleScore;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnWordProgressChanged += HandleWordProgress;
            gameManager.OnWordCompleted += HandleWordCompleted;
            gameManager.State.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            gameManager.OnScoreChanged -= HandleScore;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnWordProgressChanged -= HandleWordProgress;
            gameManager.OnWordCompleted -= HandleWordCompleted;
            gameManager.State.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            // Oyun duraklatılınca müzik de dursun, devam edince kaldığı yerden sürsün (B-01).
            if (musicSource == null) return;
            if (state == GameState.Paused) musicSource.Pause();
            else if (state == GameState.Playing) musicSource.UnPause();
        }

        private void HandleScore(int score)
        {
            // Normal skorda ding çal, ama harf sesleri zaten HandleWordProgress'te çalacak.
        }

        private void HandleWordProgress(string word, int letterIndex)
        {
            // Harf toplandıysa (başlangıçta 0'dır, her yediğinde artar)
            if (letterIndex > 0 && letterIndex - 1 < word.Length)
            {
                char letter = word[letterIndex - 1];
                // SfxGenerator'dan dinamik hece sesi üretip çalalım
                AudioClip letterSound = SfxGenerator.CreateLetterSound(letter);
                Play(letterSound);
            }
        }

        private void HandleWordCompleted(string word)
        {
            // Kelime bittiğinde Burak için neşeli bir başarı jingle'ı (applause stili) çalalım
            Play(applauseClip);
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
