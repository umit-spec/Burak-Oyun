using UnityEngine;

namespace BurakOyun.Audio
{
    public static class SfxGenerator
    {
        public static AudioClip CreateDing(float duration = 0.3f, float freq = 880f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create("Ding", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateBoing(float duration = 0.4f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create("Boing", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                float freq = 300f + 200f * Mathf.Sin(t * 25f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateApplause(float duration = 1.5f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create("Applause", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = t < 0.2f ? t / 0.2f : 1f - (t - 0.2f) / (duration - 0.2f);
                float chime = Mathf.Sin(2f * Mathf.PI * 523f * t) * 0.2f
                            + Mathf.Sin(2f * Mathf.PI * 659f * t) * 0.15f
                            + Mathf.Sin(2f * Mathf.PI * 784f * t) * 0.15f;
                float noise = (Random.value * 2f - 1f) * 0.12f;
                data[i] = (chime + noise) * envelope;
            }
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Yumuşak, rahatsız etmeyen kısa loop müzik (sakin majör akor + hafif tremolo).
        /// Frekanslar 2 sn'de tam sayıda devir tamamlar → dikişsiz (seamless) loop, tık sesi yok.
        /// Düşük genlik; AudioSource.volume ile ayrıca kısılır.
        /// </summary>
        public static AudioClip CreateMusicLoop(float duration = 2f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create("MusicLoop", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // 220 / 277 / 330 Hz (A majör hissi) — 2 sn'de tam devir; 1 Hz tremolo de tam devir
                float chord = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.5f
                            + Mathf.Sin(2f * Mathf.PI * 277f * t) * 0.35f
                            + Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.30f;
                float tremolo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 1f * t);
                data[i] = chord * 0.18f * tremolo;
            }
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateLetterSound(char letter, float duration = 0.5f)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create($"Letter_{letter}", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            float baseFreq = 260f + (letter - 'A') * 15f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = t < 0.05f ? t / 0.05f : Mathf.Max(0, 1f - (t - 0.05f) / (duration - 0.05f));
                data[i] = (Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.4f
                         + Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.2f)
                         * envelope;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
