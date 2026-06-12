using System;
using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Izgara tabanlı klasik yılan: her tick bir hücre ilerler, yem yiyince büyür,
    /// duvara/kendine çarpınca OnDied yayınlar.
    /// Görsel segmentler havuzlanır — uzun oyunlarda Instantiate/Destroy döngüsü yok.
    /// Görsel prefab'lar boş bırakılırsa salt-mantık modunda çalışır (testler).
    /// </summary>
    [RequireComponent(typeof(DirectionInput))]
    public class SnakeController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private FoodSpawner foodSpawner;

        [Header("Görseller (boş = salt-mantık modu)")]
        [SerializeField] private GameObject headPrefab;
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private float visualHeight = 0.5f;

        public event Action OnAteFood;
        public event Action OnDied;

        public GridMovement Grid { get; private set; }
        public SnakeBody Body { get; private set; }
        public Direction CurrentDirection { get; private set; } = Direction.Right;
        public bool IsMoving { get; set; }
        /// <summary>Yedikçe kısalan tick süresi (yumuşak hızlanma).</summary>
        public float CurrentTickRate { get; private set; }
        public Vector3 HeadWorldPosition => Grid.CellToWorld(Body.HeadPosition, config.cellSize, visualHeight);

        private DirectionInput input;
        private float tickTimer;
        private GameObject headVisual;
        private readonly List<GameObject> bodyVisuals = new();

        private void Awake()
        {
            input = GetComponent<DirectionInput>();
            Grid = new GridMovement(config.gridWidth, config.gridHeight);
            Body = new SnakeBody();
            ResetSnake();
        }

        /// <summary>Yılanı tahta ortasına, sağa bakar şekilde başlangıç uzunluğunda kurar.</summary>
        public void ResetSnake()
        {
            var center = new Vector2Int(config.gridWidth / 2, config.gridHeight / 2);
            CurrentDirection = Direction.Right;
            // Gövde sola uzanır; tahtadan taşmasın
            int length = Mathf.Clamp(config.initialLength, 1, center.x + 1);
            Body.Reset(center, CurrentDirection, length);
            CurrentTickRate = config.tickRate;
            tickTimer = 0f;
            input.ClearQueue();
            SyncVisuals();
        }

        private void Update()
        {
            if (!IsMoving) return;
            tickTimer += Time.deltaTime;
            if (tickTimer >= CurrentTickRate)
            {
                tickTimer -= CurrentTickRate;
                Step();
            }
        }

        /// <summary>Bir hücre ilerletir. Update tick'i çağırır; testler doğrudan çağırabilir.</summary>
        public void Step()
        {
            if (input.TryGetNextDirection(CurrentDirection, out Direction next))
                CurrentDirection = next;

            Vector2Int nextPos = Grid.GetNextPosition(Body.HeadPosition, CurrentDirection);
            bool hasFood = foodSpawner != null && foodSpawner.HasFood;
            Vector2Int foodPos = hasFood ? foodSpawner.Position : default;

            switch (CollisionManager.Check(Grid, Body, nextPos, hasFood, foodPos))
            {
                case CollisionManager.CollisionType.Wall:
                case CollisionManager.CollisionType.Self:
                    IsMoving = false;
                    OnDied?.Invoke();
                    return;

                case CollisionManager.CollisionType.Food:
                    Body.AdvanceHead(nextPos); // kuyruk silinmez → büyüme
                    CurrentTickRate = Mathf.Max(config.minTickRate, CurrentTickRate - config.speedUpPerFood);
                    OnAteFood?.Invoke();
                    break;

                default:
                    Body.AdvanceHead(nextPos);
                    Body.RemoveTail();
                    break;
            }

            SyncVisuals();
        }

        // ── Görseller (havuzlu) ──
        private void SyncVisuals()
        {
            if (segmentPrefab == null) return; // salt-mantık modu

            if (headVisual == null && headPrefab != null)
                headVisual = Instantiate(headPrefab, transform);

            if (headVisual != null)
            {
                headVisual.transform.position = Grid.CellToWorld(Body.HeadPosition, config.cellSize, visualHeight);
                Vector2Int f = GridMovement.ToVector(CurrentDirection);
                headVisual.transform.rotation = Quaternion.LookRotation(new Vector3(f.x, 0f, f.y));
            }

            int needed = Body.Length - 1; // baş hariç
            while (bodyVisuals.Count < needed)
                bodyVisuals.Add(Instantiate(segmentPrefab, transform));

            for (int i = 0; i < bodyVisuals.Count; i++)
            {
                bool active = i < needed;
                bodyVisuals[i].SetActive(active);
                if (active)
                    bodyVisuals[i].transform.position =
                        Grid.CellToWorld(Body.Segments[i + 1], config.cellSize, visualHeight);
            }
        }
    }
}
