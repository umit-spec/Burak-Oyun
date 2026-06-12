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
    /// Çekirdek oyun döngüsü smoke testi: SnakeController + FoodSpawner + GameManager'ı
    /// kodla kurup ye→büyü→skor ve duvar→oyun sonu→tekrar oyna akışını uçtan uca doğrular.
    /// Görsel prefab'lar bağlanmaz (salt-mantık modu); tick yerine Step() doğrudan çağrılır
    /// — test zamana bağımlı değildir.
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

        static GameConfig SmallConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.gridWidth = 7;
            config.gridHeight = 7;
            config.initialLength = 3;
            config.tickRate = 0.05f;
            return config;
        }

        [UnityTest]
        public IEnumerator YeBuyu_DuvaraCarp_TekrarOyna_TamDongu()
        {
            var config = SmallConfig();

            // ── Rig kurulumu ──
            var snakeGo = new GameObject("SnakeRig");
            snakeGo.SetActive(false);
            snakeGo.AddComponent<DirectionInput>();
            var snake = snakeGo.AddComponent<SnakeController>();
            SetPrivate(snake, "config", config);

            var foodGo = new GameObject("FoodRig");
            var food = foodGo.AddComponent<FoodSpawner>();
            SetPrivate(food, "config", config);
            SetPrivate(food, "snake", snake);
            SetPrivate(snake, "foodSpawner", food);

            var gmGo = new GameObject("GameManagerRig");
            gmGo.SetActive(false);
            var gm = gmGo.AddComponent<GameManager>();
            SetPrivate(gm, "snake", snake);
            SetPrivate(gm, "foodSpawner", food);

            // Ses zinciri de event'lere abone olup patlamamalı (klipler programatik üretilir)
            var audio = gmGo.AddComponent<BurakOyun.Audio.AudioManager>();
            SetPrivate(audio, "gameManager", gm);
            SetPrivate(audio, "sfxSource", gmGo.AddComponent<AudioSource>());

            snakeGo.SetActive(true);
            gmGo.SetActive(true);
            yield return null;

            int gameOverCount = 0;
            int lastScore = -1;
            gm.OnScoreChanged += s => lastScore = s;
            gm.OnGameOver += (s, best) => gameOverCount++;

            // ── Başlat ──
            gm.StartGame();
            Assert.AreEqual(GameState.Playing, gm.State.Current);
            Assert.IsTrue(snake.IsMoving);
            Assert.AreEqual(3, snake.Body.Length);
            Assert.AreEqual(new Vector2Int(3, 3), snake.Body.HeadPosition, "Yılan tahta ortasında başlamalı");
            Assert.IsTrue(food.HasFood, "Başlangıçta yem spawn edilmeli");

            // ── Ye → büyü → skor ──
            food.Clear();
            food.SpawnAt(new Vector2Int(4, 3)); // tam önüne koy (sağa bakıyor)
            snake.Step();
            Assert.AreEqual(4, snake.Body.Length, "Yem yiyince yılan büyümeli");
            Assert.AreEqual(1, gm.Score, "Yem yiyince skor artmalı");
            Assert.AreEqual(1, lastScore, "OnScoreChanged yayınlanmalı");
            Assert.IsTrue(food.HasFood, "Yem yenince yenisi spawn edilmeli");
            Assert.IsFalse(snake.Body.ContainsPosition(food.Position), "Yeni yem yılanın üstüne gelmemeli");

            // ── Duvara sür → oyun sonu ──
            // Baş (4,3)'te sağa bakıyor; 7 genişlikte tahtada en geç 3-4 adımda duvar.
            // Yolda rastgele yem çıkarsa yer ve büyür — ölüm yine de garantidir.
            for (int i = 0; i < 10 && gameOverCount == 0; i++)
                snake.Step();

            Assert.AreEqual(1, gameOverCount, "Duvara çarpınca OnGameOver tam bir kez yayınlanmalı");
            Assert.AreEqual(GameState.GameOver, gm.State.Current);
            Assert.IsFalse(snake.IsMoving, "Oyun sonunda yılan durmalı");

            // ── Tekrar oyna → her şey sıfırlanır ──
            gm.Replay();
            Assert.AreEqual(GameState.Playing, gm.State.Current);
            Assert.AreEqual(0, gm.Score, "Tekrar oynayınca skor sıfırlanmalı");
            Assert.AreEqual(3, snake.Body.Length, "Tekrar oynayınca uzunluk sıfırlanmalı");
            Assert.AreEqual(new Vector2Int(3, 3), snake.Body.HeadPosition);
            Assert.IsTrue(snake.IsMoving);
            Assert.IsTrue(food.HasFood);

            Object.Destroy(snakeGo);
            Object.Destroy(foodGo);
            Object.Destroy(gmGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator KendineCarpma_OyunuBitirir()
        {
            var config = SmallConfig();
            // Not: 4 segmentlik yılan U-dönüşünde kuyruğunu kovalar ve "kuyruk hücresi güvenli"
            // kuralıyla ÖLMEZ. Kendine çarpma için daha uzun yılan gerekir.
            config.gridWidth = 11;
            config.gridHeight = 11;
            config.initialLength = 6;

            var snakeGo = new GameObject("SnakeRig");
            snakeGo.SetActive(false);
            var input = snakeGo.AddComponent<DirectionInput>();
            var snake = snakeGo.AddComponent<SnakeController>();
            SetPrivate(snake, "config", config);

            snakeGo.SetActive(true);
            yield return null;

            int died = 0;
            snake.OnDied += () => died++;
            snake.IsMoving = true;

            // Baş (5,5), gövde sola: (4,5)(3,5)(2,5)(1,5)(0,5) — 6 segment.
            // U-dönüşü: yukarı (5,6), sola (4,6), aşağı → hedef (4,5) = gövde (kuyruk değil) → ölmeli.
            input.Enqueue(Direction.Up);
            snake.Step(); // baş (5,6)
            input.Enqueue(Direction.Left);
            snake.Step(); // baş (4,6)
            input.Enqueue(Direction.Down);
            snake.Step(); // hedef (4,5) → gövdeye çarpar

            Assert.AreEqual(1, died, "Kendine çarpınca OnDied yayınlanmalı");
            Assert.IsFalse(snake.IsMoving);

            Object.Destroy(snakeGo);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Duraklat_YilaniDurdurur_DevamKaldiginiSurdurur()
        {
            var config = SmallConfig();

            var snakeGo = new GameObject("SnakeRig");
            snakeGo.SetActive(false);
            snakeGo.AddComponent<DirectionInput>();
            var snake = snakeGo.AddComponent<SnakeController>();
            SetPrivate(snake, "config", config);

            var foodGo = new GameObject("FoodRig");
            var food = foodGo.AddComponent<FoodSpawner>();
            SetPrivate(food, "config", config);
            SetPrivate(food, "snake", snake);
            SetPrivate(snake, "foodSpawner", food);

            var gmGo = new GameObject("GameManagerRig");
            gmGo.SetActive(false);
            var gm = gmGo.AddComponent<GameManager>();
            SetPrivate(gm, "snake", snake);
            SetPrivate(gm, "foodSpawner", food);

            snakeGo.SetActive(true);
            gmGo.SetActive(true);
            yield return null;

            gm.StartGame();
            Assert.AreEqual(GameState.Playing, gm.State.Current);
            var headBefore = snake.Body.HeadPosition;

            // Duraklat: yilan durur, durum Paused, bas kimildamaz
            gm.Pause();
            Assert.IsTrue(gm.IsPaused);
            Assert.AreEqual(GameState.Paused, gm.State.Current);
            Assert.IsFalse(snake.IsMoving, "Pause yilani durdurmali");
            Assert.AreEqual(headBefore, snake.Body.HeadPosition, "Pause sirasinda bas kimildamamali");

            // Devam: hareket surer, durum Playing, kaldigi yerden ilerler
            gm.Resume();
            Assert.IsFalse(gm.IsPaused);
            Assert.AreEqual(GameState.Playing, gm.State.Current);
            Assert.IsTrue(snake.IsMoving, "Resume hareketi surdurmeli");

            snake.Step();
            Assert.AreEqual(new Vector2Int(headBefore.x + 1, headBefore.y), snake.Body.HeadPosition,
                "Resume kaldigi yerden devam etmeli (saga 1 hucre)");

            Object.Destroy(snakeGo);
            Object.Destroy(foodGo);
            Object.Destroy(gmGo);
            yield return null;
        }
    }
}
