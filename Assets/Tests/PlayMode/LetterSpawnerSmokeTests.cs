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
    /// LetterSpawner entegrasyon smoke testleri.
    /// Gerçek spawn döngüsü yerine NotifyCollected ve StopAndClear
    /// akışlarını doğrular; GameConfig/sahne gerekmez.
    /// </summary>
    public class LetterSpawnerSmokeTests
    {
        static void SetPrivate(object target, string field, object value)
        {
            var f = target.GetType().GetField(field,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"Alan bulunamadı: {target.GetType().Name}.{field}");
            f.SetValue(target, value);
        }

        // ── Test 1: NotifyCollected → WordManager.Submit() tetikler ──────────
        [UnityTest]
        public IEnumerator NotifyCollected_SubmitsLetter_ToWordManager()
        {
            var rig = new GameObject("SpawnerRig");
            rig.SetActive(false);

            var word = ScriptableObject.CreateInstance<WordData>();
            word.word = "BURAK";

            var wm = rig.AddComponent<WordManager>();
            SetPrivate(wm, "wordData", word);

            var spawner = rig.AddComponent<LetterSpawner>();
            SetPrivate(spawner, "wordManager", wm);

            rig.SetActive(true);
            yield return null;

            wm.ResetWord();
            Assert.AreEqual(0, wm.CurrentIndex, "Reset sonrası index 0 olmalı");

            // LetterCollectible'ı el ile kur
            var letterGo = new GameObject("Letter");
            letterGo.AddComponent<BoxCollider>().isTrigger = true;
            var lc = letterGo.AddComponent<LetterCollectible>();
            lc.Init('B', spawner);

            spawner.NotifyCollected(lc);

            Assert.AreEqual(1, wm.CurrentIndex, "B toplandı → index 1 olmalı");
            Assert.IsFalse(letterGo.activeSelf, "Toplanan harf deaktif edilmeli (pool'a döndü)");

            Object.Destroy(rig);
            Object.Destroy(letterGo);
            yield return null;
        }

        // ── Test 2: StopAndClear → aktif harfler deaktif edilir ──────────────
        [UnityTest]
        public IEnumerator StopAndClear_DeactivatesAllActiveLetters()
        {
            var rig = new GameObject("SpawnerRig2");
            rig.SetActive(false);

            var word = ScriptableObject.CreateInstance<WordData>();
            word.word = "AB";

            var wm = rig.AddComponent<WordManager>();
            SetPrivate(wm, "wordData", word);

            var spawner = rig.AddComponent<LetterSpawner>();
            SetPrivate(spawner, "wordManager", wm);

            rig.SetActive(true);
            yield return null;

            // Active listeye iki harf ekle (reflection ile)
            var activeField = spawner.GetType().GetField("active",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var activeList = (System.Collections.Generic.List<LetterCollectible>)
                activeField!.GetValue(spawner);

            var go1 = new GameObject("L1"); go1.AddComponent<BoxCollider>().isTrigger = true;
            var go2 = new GameObject("L2"); go2.AddComponent<BoxCollider>().isTrigger = true;
            var lc1 = go1.AddComponent<LetterCollectible>();
            var lc2 = go2.AddComponent<LetterCollectible>();
            lc1.Init('A', spawner);
            lc2.Init('B', spawner);
            activeList.Add(lc1);
            activeList.Add(lc2);

            Assert.AreEqual(2, activeList.Count, "Başlangıçta 2 aktif harf olmalı");

            spawner.StopAndClear();

            Assert.AreEqual(0, activeList.Count, "StopAndClear sonrası aktif liste boş olmalı");
            Assert.IsFalse(go1.activeSelf, "İlk harf deaktif edilmeli");
            Assert.IsFalse(go2.activeSelf, "İkinci harf deaktif edilmeli");

            Object.Destroy(rig);
            Object.Destroy(go1);
            Object.Destroy(go2);
            yield return null;
        }

        // ── Test 3: LetterCollectible.Init — label null'sa crash yok ─────────
        [UnityTest]
        public IEnumerator LetterCollectible_Init_WithNullLabel_DoesNotThrow()
        {
            var go = new GameObject("LetterNoLabel");
            go.AddComponent<BoxCollider>().isTrigger = true;
            var lc = go.AddComponent<LetterCollectible>();

            // label SerializeField null — Init çökmemeli
            Assert.DoesNotThrow(() => lc.Init('K', null));
            Assert.AreEqual('K', lc.Letter);

            // OnTriggerEnter owner null'sa collect olmaz
            // (dolaylı test: collected tetiklenirse NullRef olmaz çünkü owner null guard var)

            Object.Destroy(go);
            yield return null;
        }
    }
}
