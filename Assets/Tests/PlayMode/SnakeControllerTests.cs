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

        static GameConfig MakeConfig(int w = 10, int h = 10, float tick = 0.02f)
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            cfg.gridWidth        = w;
            cfg.gridHeight       = h;
            cfg.cellSize         = 1.5f;
            cfg.tickInterval     = tick;
            cfg.minTickInterval  = 0.01f;
            cfg.decoyCount       = 2;
            cfg.decoyAlphabet    = "ABC";
            return cfg;
        }

        static (SnakeController snake, GridBoard board, GameObject snakeGo) MakeSnake(GameConfig cfg)
        {
            var boardGo = new GameObject("Board");
            boardGo.SetActive(false);
            var board = boardGo.AddComponent<GridBoard>();
            SetPrivate(board, "config", cfg);
            boardGo.SetActive(true);

            var snakeGo = new GameObject("Snake");
            snakeGo.SetActive(false);
            var snake = snakeGo.AddComponent<SnakeController>();
            SetPrivate(snake, "config", cfg);
            SetPrivate(snake, "board", board);
            snakeGo.SetActive(true);

            return (snake, board, snakeGo);
        }

        // ── Test 1: ResetToStart baş hücresini ızgaranın içine koyar ─────────
        [UnityTest]
        public IEnumerator ResetToStart_HeadIsInsideGrid()
        {
            var cfg = MakeConfig();
            var (snake, board, snakeGo) = MakeSnake(cfg);
            yield return null;

            snake.ResetToStart();

            Assert.IsTrue(board.InBounds(snake.HeadCell),
                "ResetToStart sonrası baş hücresi ızgara sınırları içinde olmalı");

            Object.Destroy(snakeGo);
            Object.Destroy(board.gameObject);
            yield return null;
        }

        // ── Test 2: Başlangıçta gövde 3 hücre ───────────────────────────────
        [UnityTest]
        public IEnumerator ResetToStart_BodyHasThreeCells()
        {
            var cfg = MakeConfig();
            var (snake, board, snakeGo) = MakeSnake(cfg);
            yield return null;

            snake.ResetToStart();

            Assert.AreEqual(3, snake.BodyLength,
                "ResetToStart sonrası yılan 3 hücre uzunluğunda başlamalı");

            Object.Destroy(snakeGo);
            Object.Destroy(board.gameObject);
            yield return null;
        }

        // ── Test 3: 180° ters yön engellenir ────────────────────────────────
        [UnityTest]
        public IEnumerator SetDirection_Prevents_180Reversal()
        {
            var cfg = MakeConfig(w: 15, h: 15, tick: 0.5f); // yavaş tick - hareketi kontrol edelim
            var (snake, board, snakeGo) = MakeSnake(cfg);
            yield return null;

            snake.ResetToStart();
            // Başlangıç yönü (0,1) = yukarı
            // Aşağı (0,-1) gitmek istiyoruz → 180° → bloke
            var headBefore = snake.HeadCell;

            snake.IsMoving = false; // tick çalıştırma
            snake.SetDirection(Vector2Int.down); // 180° → bloke olmalı

            // queuedDir'i reflection ile kontrol et
            var f = typeof(SnakeController).GetField("queuedDir",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var queued = (Vector2Int)f.GetValue(snake);

            // hasPending false ise yön kabul edilmedi
            var pf = typeof(SnakeController).GetField("hasPending",
                BindingFlags.NonPublic | BindingFlags.Instance);
            bool hasPending = (bool)pf.GetValue(snake);

            Assert.IsFalse(hasPending,
                "180° ters yön SetDirection tarafından reddedilmeli");

            Object.Destroy(snakeGo);
            Object.Destroy(board.gameObject);
            yield return null;
        }

        // ── Test 4: Grow() → bir sonraki tickte gövde uzar ─────────────────
        [UnityTest]
        public IEnumerator Grow_IncreasesBodyLength()
        {
            var cfg = MakeConfig(w: 15, h: 15, tick: 0.03f);
            var (snake, board, snakeGo) = MakeSnake(cfg);
            yield return null;

            snake.ResetToStart();
            snake.IsMoving = true;
            int before = snake.BodyLength; // 3

            snake.Grow();
            yield return new WaitForSeconds(cfg.tickInterval * 2f);

            Assert.Greater(snake.BodyLength, before,
                "Grow() çağrısından sonra gövde uzunluğu artmalı");

            Object.Destroy(snakeGo);
            Object.Destroy(board.gameObject);
            yield return null;
        }

        // ── Test 5: Duvar çarpışması OnDied tetikler ve IsMoving = false ────
        [UnityTest]
        public IEnumerator WallCollision_FiresOnDied()
        {
            // 5x5 küçük ızgara, hızlı tick
            var cfg = MakeConfig(w: 5, h: 5, tick: 0.03f);
            var (snake, board, snakeGo) = MakeSnake(cfg);
            yield return null;

            snake.ResetToStart();

            bool diedFired = false;
            snake.OnDied += () => diedFired = true;

            snake.IsMoving = true;
            // Başlangıç yönü yukarı (0,1); 5x5 ızgarada y=1'den başlayıp
            // 4 tick sonra y=5 (sınır dışı)  → ölüm
            yield return new WaitForSeconds(cfg.tickInterval * 10f);

            Assert.IsTrue(diedFired,  "Duvar çarpışması OnDied tetiklemeli");
            Assert.IsFalse(snake.IsMoving, "Ölüm sonrası IsMoving false olmalı");

            Object.Destroy(snakeGo);
            Object.Destroy(board.gameObject);
            yield return null;
        }
    }
}
