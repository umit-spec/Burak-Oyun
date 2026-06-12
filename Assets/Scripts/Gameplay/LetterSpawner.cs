using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Dalga mantığı: her dalgada 1 DOĞRU harf + 1-2 çeldirici, 3 şeride rastgele.
    /// Doğru harf her dalgada garantili → çocuk asla kilitlenmez.
    /// Object pooling ile harfler geri dönüştürülür.
    /// </summary>
    public class LetterSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private WordManager wordManager;
        [SerializeField] private SnakeController snake;
        [SerializeField] private LetterCollectible letterPrefab;
        [SerializeField] private float letterHeight = 1f;

        private readonly List<LetterCollectible> active = new();
        private readonly Queue<LetterCollectible> pool = new();
        private float spawnTimer;
        private bool running;

        public System.Action<LetterCollectible> OnLetterCollected;

        public void StartSpawning()
        {
            running = true;
            spawnTimer = 1.5f; // ilk dalga çabuk gelsin, çocuk beklemesin
        }

        public void StopAndClear()
        {
            running = false;
            for (int i = active.Count - 1; i >= 0; i--) Despawn(active[i]);
        }

        private void Update()
        {
            if (!running) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && !wordManager.IsComplete)
            {
                SpawnWave();
                spawnTimer = config.spawnInterval;
            }

            // Arkada kalanları geri havuza al
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (snake.transform.position.z - active[i].transform.position.z > config.despawnBehind)
                    Despawn(active[i]);
            }
        }

        private void SpawnWave()
        {
            char target = wordManager.TargetLetter;
            var lanes = new List<int> { -1, 0, 1 };
            Shuffle(lanes);

            Spawn(target, lanes[0]);
            for (int i = 0; i < config.decoyCount && i + 1 < lanes.Count; i++)
                Spawn(PickDecoy(target), lanes[i + 1]);
        }

        private char PickDecoy(char target)
        {
            string pool = config.decoyAlphabet;
            char c;
            do { c = pool[Random.Range(0, pool.Length)]; } while (c == target);
            return c;
        }

        private void Spawn(char letter, int lane)
        {
            LetterCollectible item = pool.Count > 0 ? pool.Dequeue() : Instantiate(letterPrefab, transform);
            item.gameObject.SetActive(true);
            item.transform.position = new Vector3(
                lane * config.laneWidth, letterHeight,
                snake.transform.position.z + config.spawnDistance);
            item.Init(letter, this);
            active.Add(item);
        }

        public void NotifyCollected(LetterCollectible item)
        {
            OnLetterCollected?.Invoke(item);
            wordManager.Submit(item.Letter);
            Despawn(item);
        }

        private void Despawn(LetterCollectible item)
        {
            active.Remove(item);
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }

        private static void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
