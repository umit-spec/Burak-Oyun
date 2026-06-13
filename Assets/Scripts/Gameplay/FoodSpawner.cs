using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;
using BurakOyun.Core;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Tek seferde tek yem. Yılanın üstüne gelmeyen rastgele BOŞ hücre seçer
    /// (boş hücreler listelenir — sonsuz deneme döngüsü yok).
    /// Görsel tek obje yeniden konumlanır; Instantiate/Destroy döngüsü yok.
    /// </summary>
    public class FoodSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private SnakeController snake;
        [SerializeField] private GameManager gameManager; // harf gösterimi için (boş = salt-mantık modu)
        [SerializeField] private GameObject foodPrefab; // boş = salt-mantık modu (testler)
        [SerializeField] private float foodHeight = 0.5f;
        [SerializeField] private float spinSpeed = 90f;

        private GameObject current;

        public bool HasFood { get; private set; }
        public Vector2Int Position { get; private set; }

        /// <summary>Rastgele güvenli hücreye yem koyar. Boş hücre yoksa false (tahta doldu).</summary>
        public bool SpawnFood()
        {
            GridMovement grid = snake.Grid;
            var free = new List<Vector2Int>();
            for (int x = 0; x < grid.GridSize.x; x++)
                for (int y = 0; y < grid.GridSize.y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!snake.Body.ContainsPosition(cell)) free.Add(cell);
                }

            if (free.Count == 0)
            {
                Clear();
                return false;
            }

            SpawnAt(free[Random.Range(0, free.Count)]);
            return true;
        }

        /// <summary>Belirli hücreye yem koyar (testler için de kullanılır).</summary>
        public void SpawnAt(Vector2Int cell)
        {
            Position = cell;
            HasFood = true;
            if (foodPrefab == null) return;
            if (current == null) current = Instantiate(foodPrefab, transform);
            current.SetActive(true);
            current.transform.position = snake.Grid.CellToWorld(cell, config.cellSize, foodHeight);

            // 3D Harf gösterimini güncelle (TextMeshPro varsa)
            var tmp = current.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp == null) tmp = current.GetComponent<TMPro.TMP_Text>();
            
            if (tmp != null)
            {
                // GameManager'dan o anki kelimeyi ve harf indeksini çekip yazdır.
                // Doğrudan referans (her spawn'da FindFirstObjectByType sahne taraması yok).
                if (gameManager != null)
                {
                    string word = gameManager.currentWord;
                    int idx = gameManager.currentLetterIndex;
                    if (!string.IsNullOrEmpty(word) && idx < word.Length)
                    {
                        tmp.text = word[idx].ToString();
                    }
                }
                else
                {
                    tmp.text = "*";
                }
            }
        }

        /// <summary>Yemi kaldırır (yenince / oyun bitince).</summary>
        public void Clear()
        {
            HasFood = false;
            if (current != null) current.SetActive(false);
        }

        private void Update()
        {
            if (!HasFood || current == null) return;
            // Sevimli dönüş + hafif nabız
            current.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
            current.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 4f) * 0.08f);
        }
    }
}
