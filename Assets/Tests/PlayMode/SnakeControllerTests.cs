using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BurakOyun.Gameplay;
using BurakOyun.Data;

namespace BurakOyun.Tests.PlayMode
{
    public class SnakeControllerTests
    {
        static void SetPrivate(object target, string field, object value)
        {
            var f = target.GetType().GetField(field,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"Alan bulunamadı: {target.GetType().Name}.{field}");
            f.SetValue(target, value);
        }

        static GameConfig MakeConfig(float speed = 4f, float slowdown = 0.5f)
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            cfg.forwardSpeed = speed;
            cfg.slowdownFactor = slowdown;
            cfg.slowdownDuration = 2f;
            cfg.laneWidth = 2.5f;
            cfg.laneChangeSpeed = 8f;
            return cfg;
        }

        // ── Test 1: Yavaşlama yokken hız tam forwardSpeed ───────────────────
        [UnityTest]
        public IEnumerator CurrentSpeed_IsNormal_WithoutSlowdown()
        {
            var go = new GameObject("Snake");
            go.SetActive(false);
            go.AddComponent<BoxCollider>();

            var snake = go.AddComponent<SnakeController>();
            SetPrivate(snake, "config", MakeConfig(speed: 5f, slowdown: 0.5f));

            go.SetActive(true);
            yield return null;

            Assert.AreEqual(5f, snake.CurrentSpeed, 0.001f,
                "Yavaşlama yokken hız forwardSpeed'e eşit olmalı");

            Object.Destroy(go);
            yield return null;
        }

        // ── Test 2: ApplySlowdown sonrası hız azalır ─────────────────────────
        [UnityTest]
        public IEnumerator CurrentSpeed_IsReduced_AfterApplySlowdown()
        {
            var go = new GameObject("SnakeSlow");
            go.SetActive(false);
            go.AddComponent<BoxCollider>();

            var snake = go.AddComponent<SnakeController>();
            var cfg = MakeConfig(speed: 4f, slowdown: 0.75f);
            SetPrivate(snake, "config", cfg);

            go.SetActive(true);
            yield return null;

            snake.ApplySlowdown();

            Assert.AreEqual(cfg.forwardSpeed * cfg.slowdownFactor, snake.CurrentSpeed, 0.001f,
                "ApplySlowdown sonrası hız forwardSpeed * slowdownFactor olmalı");

            Object.Destroy(go);
            yield return null;
        }

        // ── Test 3: ResetToStart → spawn pozisyonuna döner (X=0, Z=spawnZ) ──
        [UnityTest]
        public IEnumerator ResetToStart_ResetsToSpawnPosition()
        {
            var go = new GameObject("SnakeReset");
            go.SetActive(false);
            go.AddComponent<BoxCollider>();

            var snake = go.AddComponent<SnakeController>();
            SetPrivate(snake, "config", MakeConfig());

            // Spawn pozisyonunu kur; SetActive Awake'i tetikler ve spawnPosition'ı yakalar
            go.transform.position = new Vector3(0f, 1f, 5f);
            go.SetActive(true);
            yield return null;

            // Yılanın ilerleyişini simüle et
            go.transform.position = new Vector3(2.5f, 1f, 42f);

            snake.ResetToStart();

            var pos = go.transform.position;
            Assert.AreEqual(0f, pos.x, 0.001f, "ResetToStart sonrası X = 0 olmalı");
            Assert.AreEqual(5f, pos.z, 0.001f, "ResetToStart başlangıç Z'sine dönmeli");
            Assert.AreEqual(1f, pos.y, 0.001f, "ResetToStart Y pozisyonunu korumalı");

            Object.Destroy(go);
            yield return null;
        }
    }
}
