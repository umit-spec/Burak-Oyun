using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BurakOyun.Gameplay;
using BurakOyun.Core;
using BurakOyun.Data;

namespace BurakOyun.Tests.PlayMode
{
    /// <summary>
    /// Core loop event akışı smoke testi: spawn mantığı olmadan, WordManager + RewardManager'ı
    /// kodla kurup doğru/yanlış/tamamlanma + yıldız akışını uçtan uca doğrular.
    /// (Private SerializeField alanları reflection ile bağlanır; GameObject inactive kurulup
    ///  SetActive(true) ile Awake/OnEnable doğru referanslarla tetiklenir.)
    /// </summary>
    public class CoreLoopSmokeTests
    {
        static void SetPrivate(object target, string field, object value)
        {
            var f = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"Private alan bulunamadı: {target.GetType().Name}.{field}");
            f.SetValue(target, value);
        }

        [UnityTest]
        public IEnumerator BURAK_DogruSirada_Tamamlanir_Ve_YildizKazandirir()
        {
            var go = new GameObject("TestRig");
            go.SetActive(false); // alanları bağlamadan Awake/OnEnable tetiklenmesin

            var word = ScriptableObject.CreateInstance<WordData>();
            word.word = "BURAK";

            var wm = go.AddComponent<WordManager>();
            SetPrivate(wm, "wordData", word);

            var reward = go.AddComponent<RewardManager>();
            SetPrivate(reward, "wordManager", wm);
            SetPrivate(reward, "snake", go.transform); // sparkle pozisyonu için (null-guard'lı)

            int correct = 0, wrong = 0, complete = 0;
            wm.OnCorrectLetter += (c, i) => correct++;
            wm.OnWrongLetter += c => wrong++;
            wm.OnWordComplete += () => complete++;

            go.SetActive(true); // Awake + OnEnable: referanslar bağlı
            yield return null;

            wm.ResetWord();

            // Yanlış harf → index sabit, yanlış event, tamamlanmadı
            wm.Submit('Z');
            Assert.AreEqual(1, wrong, "Yanlış harf event'i yayınlanmalı");
            Assert.AreEqual(0, wm.CurrentIndex, "Yanlış harf index'i değiştirmemeli (geri sarma yok)");
            Assert.IsFalse(wm.IsComplete);

            // BURAK sırayla → tamamlanır
            foreach (char c in "BURAK") wm.Submit(c);

            Assert.IsTrue(wm.IsComplete, "Kelime tamamlanmalı");
            Assert.AreEqual(1, complete, "OnWordComplete tam olarak bir kez yayınlanmalı");
            Assert.AreEqual(5, correct, "5 doğru harf event'i yayınlanmalı");
            Assert.AreEqual(5, reward.Stars, "Her doğru harf +1 yıldız");

            // Reset → sıfırlanır
            wm.ResetWord();
            Assert.AreEqual(0, wm.CurrentIndex, "Reset index'i sıfırlamalı");
            Assert.AreEqual(0, reward.Stars, "Reset yıldızları sıfırlamalı");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AudioManager_KlipsizBaslar_HataVermez()
        {
            // AudioManager Awake'te SfxGenerator ile klip üretmeli, null referansla patlamamalı
            var go = new GameObject("AudioRig");
            go.SetActive(false);

            var word = ScriptableObject.CreateInstance<WordData>();
            word.word = "BURAK";

            var wm = go.AddComponent<WordManager>();
            SetPrivate(wm, "wordData", word);

            var audio = go.AddComponent<BurakOyun.Audio.AudioManager>();
            SetPrivate(audio, "wordManager", wm);
            var src = go.AddComponent<AudioSource>();
            SetPrivate(audio, "sfxSource", src);
            SetPrivate(audio, "voiceSource", src);

            go.SetActive(true);
            yield return null;

            // Olaylar tetiklendiğinde exception olmamalı (klipler programatik üretiliyor)
            Assert.DoesNotThrow(() =>
            {
                wm.ResetWord();
                wm.Submit('B');
                wm.Submit('Z');
            });

            Object.Destroy(go);
            yield return null;
        }
    }
}
