using UnityEngine;
using BurakOyun.Gameplay;

namespace BurakOyun.Audio
{
    /// <summary>
    /// Tüm sesler lokal AudioClip. Yumuşak SFX — korkutucu ses yasak.
    /// Harf sesleri WordData'dan gelir (char → clip).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private WordManager wordManager;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX (yumuşak!)")]
        [SerializeField] private AudioClip dingClip;     // doğru harf
        [SerializeField] private AudioClip boingClip;    // yanlış harf (komik, korkutucu değil)
        [SerializeField] private AudioClip applauseClip; // kelime tamam

        private void Awake()
        {
            if (dingClip == null) dingClip = SfxGenerator.CreateDing();
            if (boingClip == null) boingClip = SfxGenerator.CreateBoing();
            if (applauseClip == null) applauseClip = SfxGenerator.CreateApplause();
            if (musicSource != null && musicSource.clip == null) musicSource.clip = SfxGenerator.CreateBackgroundMusic();
        }

        private void Start()
        {
            if (musicSource != null && musicSource.clip != null) musicSource.Play();
        }

        private void OnEnable()
        {
            wordManager.OnCorrectLetter += HandleCorrect;
            wordManager.OnWrongLetter += HandleWrong;
            wordManager.OnWordComplete += HandleComplete;
        }

        private void OnDisable()
        {
            wordManager.OnCorrectLetter -= HandleCorrect;
            wordManager.OnWrongLetter -= HandleWrong;
            wordManager.OnWordComplete -= HandleComplete;
        }

        private void HandleCorrect(char letter, int index)
        {
            Play(sfxSource, dingClip);
            var clip = wordManager.WordData != null ? wordManager.WordData.GetLetterClip(letter) : null;
            if (clip == null) clip = SfxGenerator.CreateLetterSound(letter);
            Play(voiceSource, clip);
        }

        private void HandleWrong(char letter) => Play(sfxSource, boingClip);

        private void HandleComplete()
        {
            Play(sfxSource, applauseClip);
            if (wordManager.WordData != null)
                Play(voiceSource, wordManager.WordData.wordAudio);
        }

        private static void Play(AudioSource source, AudioClip clip)
        {
            if (source != null && clip != null) source.PlayOneShot(clip);
        }
    }
}
