using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// Izgara tabanlı harf spawn: rastgele boş hücreye koy, yılan topladığında sil.
    public class LetterSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private WordManager wordManager;
        [SerializeField] private SnakeController snake;
        [SerializeField] private GridBoard board;
        [SerializeField] private LetterCollectible letterPrefab;
        [SerializeField] private float letterHeight = 0.75f;

        private readonly List<LetterCollectible> active = new();
        private readonly Dictionary<LetterCollectible, Vector2Int> letterCells = new();
        private readonly Queue<LetterCollectible> pool = new();
        private bool running;
        private float spawnDelay;

        public void StartSpawning()
        {
            running    = true;
            spawnDelay = 0.8f;
        }

        public void StopAndClear()
        {
            running = false;
            for (int i = active.Count - 1; i >= 0; i--) Despawn(active[i]);
        }

        private void Update()
        {
            if (!running || wordManager.IsComplete) return;
            if (active.Count > 0) return;

            spawnDelay -= Time.deltaTime;
            if (spawnDelay <= 0f) SpawnWave();
        }

        private void SpawnWave()
        {
            if (wordManager.IsComplete) return;

            var occupied = new HashSet<Vector2Int>(snake.BodyCells);
            foreach (var cell in letterCells.Values) occupied.Add(cell);

            char target = wordManager.TargetLetter;
            int total   = 1 + config.decoyCount;
            var cells   = new List<Vector2Int>(total);

            for (int i = 0; i < total; i++)
            {
                var c = board.RandomEmpty(occupied);
                cells.Add(c);
                occupied.Add(c);
            }

            Shuffle(cells);
            Spawn(target, cells[0]);
            for (int i = 1; i <= config.decoyCount && i < cells.Count; i++)
                Spawn(PickDecoy(target), cells[i]);
        }

        private void Spawn(char letter, Vector2Int cell)
        {
            LetterCollectible item = pool.Count > 0
                ? pool.Dequeue()
                : Instantiate(letterPrefab, transform);
            item.gameObject.SetActive(true);
            item.transform.position = board.CellToWorld(cell) + Vector3.up * letterHeight;
            item.Init(letter, this);
            active.Add(item);
            letterCells[item] = cell;
        }

        public void NotifyCollected(LetterCollectible item)
        {
            bool wasTarget = item.Letter == wordManager.TargetLetter;
            wordManager.Submit(item.Letter);
            Despawn(item);

            if (wasTarget && running && !wordManager.IsComplete)
            {
                // Kalan çeldiricileri temizle, biraz bekleyip yeni dalga aç
                for (int i = active.Count - 1; i >= 0; i--) Despawn(active[i]);
                spawnDelay = 0.5f;
            }
        }

        private void Despawn(LetterCollectible item)
        {
            active.Remove(item);
            letterCells.Remove(item);
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }

        private char PickDecoy(char target)
        {
            string alphabet = config.decoyAlphabet;
            char c;
            do { c = alphabet[Random.Range(0, alphabet.Length)]; } while (c == target);
            return c;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
