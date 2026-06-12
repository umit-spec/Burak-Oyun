namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Saf C# kelime ilerleme çekirdeği — Unity'siz, unit-test edilebilir.
    /// Yanlış harf indexi GERİ SARMAZ (çocuk dostu: ilerleme asla kaybolmaz).
    /// </summary>
    public class WordProgress
    {
        public string Word { get; }
        public int CurrentIndex { get; private set; }
        public bool IsComplete => CurrentIndex >= Word.Length;
        public char TargetLetter => IsComplete ? '\0' : Word[CurrentIndex];
        public string CollectedPart => Word.Substring(0, CurrentIndex);

        public WordProgress(string word)
        {
            Word = word ?? string.Empty;
            CurrentIndex = 0;
        }

        public enum SubmitResult { Correct, Wrong, WordComplete, AlreadyComplete }

        public SubmitResult TrySubmit(char letter)
        {
            if (IsComplete) return SubmitResult.AlreadyComplete;
            if (letter != Word[CurrentIndex]) return SubmitResult.Wrong;
            CurrentIndex++;
            return IsComplete ? SubmitResult.WordComplete : SubmitResult.Correct;
        }

        public void Reset() => CurrentIndex = 0;
    }
}
