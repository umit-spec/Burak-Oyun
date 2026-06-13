using System.Collections.Generic;
using UnityEngine;

namespace BurakOyun.Audio
{
    public static class SfxGenerator
    {
        // Her harf için AudioClip bir kez üretilip cache'de tutulur.
        // Unity domain reload'da static alan sıfırlanır, sahne geçişinde sorun olmaz.
        private static readonly Dictionary<char, AudioClip> s_letterCache = new();

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

        public static AudioClip CreateLetterSound(char letter, float duration = 0.5f)
        {
            if (s_letterCache.TryGetValue(letter, out AudioClip cached) && cached != null)
                return cached;

            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var clip = AudioClip.Create($"Letter_{letter}", samples, 1, sampleRate, false);
            float[] data = new float[samples];
            float baseFreq = 260f + (letter - 'A') * 15f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = t < 0.05f
                    ? t / 0.05f
                    : Mathf.Max(0, 1f - (t - 0.05f) / (duration - 0.05f));
                data[i] = (Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.4f
                         + Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.2f)
                         * envelope;
            }
            clip.SetData(data, 0);
            s_letterCache[letter] = clip;
            return clip;
        }

        public static AudioClip CreateBackgroundMusic(float bpm = 100f)
        {
            int sampleRate = 44100;
            float[] freqs = { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f };
            int[] melody = { 2, 2, 3, 4,  3, 2, 0, 2,   // Bar 1
                             3, 3, 4, 4,  3, 2, 3, -1,  // Bar 2
                             2, 2, 3, 4,  3, 2, 0, 2,   // Bar 3
                             0, 1, 2, -1, -1,-1,-1,-1 }; // Bar 4

            float eighthNote = 30f / bpm;
            float totalDur = melody.Length * eighthNote;
            int totalSamples = (int)(sampleRate * totalDur);
            float[] data = new float[totalSamples];

            int attackSamples = (int)(0.01f * sampleRate); // 10ms attack

            for (int n = 0; n < melody.Length; n++)
            {
                int startSample = (int)(n * eighthNote * sampleRate);
                int noteSamples = (int)(eighthNote * sampleRate);

                // Melody note
                if (melody[n] >= 0)
                {
                    float freq = freqs[melody[n]];
                    for (int i = 0; i < noteSamples; i++)
                    {
                        int sampleIndex = startSample + i;
                        if (sampleIndex >= totalSamples) break;
                        float t = (float)i / sampleRate;
                        float attack = i < attackSamples ? (float)i / attackSamples : 1f;
                        float release = 1f - (float)i / noteSamples;
                        float env = attack * release;
                        data[sampleIndex] += Mathf.Sin(2f * Mathf.PI * freq * t) * 0.13f * env
                                           + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.05f * env;
                    }
                }

                // Bass note on every 4th note
                if (n % 4 == 0)
                {
                    float bassFreq = freqs[0] * 0.5f; // C3
                    int bassSamples = (int)(eighthNote * 2f * sampleRate); // half-note duration
                    for (int i = 0; i < bassSamples; i++)
                    {
                        int sampleIndex = startSample + i;
                        if (sampleIndex >= totalSamples) break;
                        float t = (float)i / sampleRate;
                        float attack = i < attackSamples ? (float)i / attackSamples : 1f;
                        float release = 1f - (float)i / bassSamples;
                        float env = attack * release;
                        data[sampleIndex] += Mathf.Sin(2f * Mathf.PI * bassFreq * t) * 0.07f * env;
                    }
                }
            }

            var clip = AudioClip.Create("BgMusic", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
