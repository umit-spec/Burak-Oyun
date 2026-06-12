using System;
using UnityEngine;

namespace BurakOyun.Data
{
    /// <summary>
    /// Bir hedef kelime + seslendirmeleri. Yeni kelime = yeni asset, kod değişmez.
    /// Tüm sesler lokal AudioClip — internet/TTS servisi kullanılmaz.
    /// </summary>
    [CreateAssetMenu(fileName = "WordData", menuName = "BurakOyun/Word Data")]
    public class WordData : ScriptableObject
    {
        [Tooltip("Hedef kelime, BÜYÜK harf. Örn: BURAK")]
        public string word = "BURAK";

        [Tooltip("Kelime tamamlanınca çalınacak tam okuma sesi.")]
        public AudioClip wordAudio;

        [Tooltip("Her harf için seslendirme klibi.")]
        public LetterAudioEntry[] letters;

        public AudioClip GetLetterClip(char letter)
        {
            if (letters == null) return null;
            foreach (var entry in letters)
                if (entry.letter.Length > 0 && entry.letter[0] == letter)
                    return entry.clip;
            return null;
        }
    }

    [Serializable]
    public struct LetterAudioEntry
    {
        [Tooltip("Tek büyük harf, örn: B")]
        public string letter;
        public AudioClip clip;
    }
}
