using System;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Hedef kelimeyi yönetir, harf gönderimlerini değerlendirir, event yayınlar.
    /// Diğer sistemler (UI, ödül, ses) bu event'lere abone olur — doğrudan çağrı yok.
    /// </summary>
    public class WordManager : MonoBehaviour
    {
        [SerializeField] private WordData wordData;

        public event Action<char, int> OnCorrectLetter;   // harf, yeni index
        public event Action<char> OnWrongLetter;
        public event Action OnWordComplete;
        public event Action OnWordReset;

        private WordProgress progress;

        public WordData WordData => wordData;
        public string Word => progress?.Word ?? "";
        public int CurrentIndex => progress?.CurrentIndex ?? 0;
        public char TargetLetter => progress?.TargetLetter ?? '\0';
        public bool IsComplete => progress?.IsComplete ?? false;

        private void Awake()
        {
            progress = new WordProgress(wordData != null ? wordData.word : "BURAK");
        }

        public void Submit(char letter)
        {
            switch (progress.TrySubmit(letter))
            {
                case WordProgress.SubmitResult.Correct:
                    OnCorrectLetter?.Invoke(letter, progress.CurrentIndex);
                    break;
                case WordProgress.SubmitResult.Wrong:
                    OnWrongLetter?.Invoke(letter);
                    break;
                case WordProgress.SubmitResult.WordComplete:
                    OnCorrectLetter?.Invoke(letter, progress.CurrentIndex);
                    OnWordComplete?.Invoke();
                    break;
            }
        }

        public void ResetWord()
        {
            progress.Reset();
            OnWordReset?.Invoke();
        }
    }
}
