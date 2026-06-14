using UnityEngine;
using BurakOyun.Gameplay;

namespace BurakOyun.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private WordManager wordManager;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX")]
        [SerializeField] private AudioClip dingClip;
        [SerializeField] private AudioClip boingClip;
        [SerializeField] private AudioClip applauseClip;
        [SerializeField] private AudioClip deathClip;

        private bool sfxMuted;
        private bool musicMuted;

        private void Awake()
        {
            if (dingClip == null)     dingClip     = SfxGenerator.CreateDing();
            if (boingClip == null)    boingClip    = SfxGenerator.CreateBoing();
            if (applauseClip == null) applauseClip = SfxGenerator.CreateApplause();
            if (deathClip == null)    deathClip    = SfxGenerator.CreateCrash();
            if (musicSource != null && musicSource.clip == null)
                musicSource.clip = SfxGenerator.CreateBackgroundMusic();

            // Kayıtlı tercihlerle başla
            sfxMuted   = PlayerPrefs.GetInt("SfxMuted",   0) == 1;
            musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            ApplyMuteState();
        }

        private void Start()
        {
            if (musicSource != null && musicSource.clip != null && !musicMuted)
                musicSource.Play();
        }

        private void OnEnable()
        {
            if (wordManager == null) { Debug.LogError("[AudioManager] wordManager atanmamış!"); return; }
            wordManager.OnCorrectLetter += HandleCorrect;
            wordManager.OnWrongLetter   += HandleWrong;
            wordManager.OnWordComplete  += HandleComplete;
        }

        private void OnDisable()
        {
            if (wordManager == null) return;
            wordManager.OnCorrectLetter -= HandleCorrect;
            wordManager.OnWrongLetter   -= HandleWrong;
            wordManager.OnWordComplete  -= HandleComplete;
        }

        public void ToggleSound()
        {
            sfxMuted = !sfxMuted;
            musicMuted = sfxMuted;
            ApplyMuteState();
            PlayerPrefs.SetInt("SfxMuted",   sfxMuted   ? 1 : 0);
            PlayerPrefs.SetInt("MusicMuted", musicMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool IsMuted => sfxMuted;

        private void ApplyMuteState()
        {
            if (sfxSource   != null) sfxSource.mute   = sfxMuted;
            if (voiceSource != null) voiceSource.mute  = sfxMuted;
            if (musicSource != null) musicSource.mute  = musicMuted;
        }

        public void PlayDeath() => Play(sfxSource, deathClip);

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
