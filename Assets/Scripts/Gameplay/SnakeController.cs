using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    [RequireComponent(typeof(LaneInput))]
    public class SnakeController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private GridBoard board;

        public event System.Action OnDied;

        private readonly LinkedList<Vector2Int> body = new();
        private Vector2Int direction  = new(0, 1);
        private Vector2Int queuedDir  = new(0, 1);
        private bool hasPending;
        private bool isMoving;
        private bool growNext;
        private float tickTimer;
        private float slowdownTimer;
        private int boostTicks;

        public bool IsMoving
        {
            get => isMoving;
            set { isMoving = value; if (value) tickTimer = 0f; }
        }

        public Vector2Int HeadCell => body.Count > 0 ? body.First.Value : new(config.gridWidth / 2, 2);
        public IEnumerable<Vector2Int> BodyCells => body;
        public int BodyLength => body.Count;

        public void ResetToStart()
        {
            body.Clear();
            int cx = config.gridWidth  / 2;
            int cy = config.gridHeight / 4;
            body.AddFirst(new Vector2Int(cx, cy));
            body.AddLast(new Vector2Int(cx, cy - 1));
            body.AddLast(new Vector2Int(cx, cy - 2));
            direction     = new Vector2Int(0, 1);
            queuedDir     = direction;
            hasPending    = false;
            growNext      = false;
            boostTicks    = 0;
            slowdownTimer = 0f;
            tickTimer     = 0f;
            SyncVisuals();
        }

        public void SetDirection(Vector2Int dir)
        {
            // 180° dönüş yasağı
            if (dir + direction == Vector2Int.zero) return;
            queuedDir  = dir;
            hasPending = true;
        }

        public void Grow() => growNext = true;

        public void AddSpeedBoost()
        {
            boostTicks    = Mathf.Min(boostTicks + 3, 15);
            slowdownTimer = 0f;
        }

        public void ApplySlowdown()
        {
            slowdownTimer = 2f;
            boostTicks    = 0;
        }

        private void Update()
        {
            if (!isMoving) return;

            float interval = config.tickInterval;
            if      (boostTicks > 0)      interval = Mathf.Max(interval * 0.65f, config.minTickInterval);
            else if (slowdownTimer > 0f)  interval = interval * 1.5f;

            if (slowdownTimer > 0f) slowdownTimer -= Time.deltaTime;

            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = interval;
                Tick();
            }
        }

        private void Tick()
        {
            if (hasPending) { direction = queuedDir; hasPending = false; }
            if (boostTicks > 0) boostTicks--;

            var newHead = HeadCell + direction;

            if (!board.InBounds(newHead)) { Die(); return; }

            // Kendi kendine çarpışma: kuyruk bir adım sonra kayacak → yeni baş oraya girebilir
            var tail = body.Last.Value;
            foreach (var cell in body)
            {
                if (cell == newHead)
                {
                    if (cell == tail && !growNext) continue;
                    Die(); return;
                }
            }

            body.AddFirst(newHead);
            if (growNext) growNext = false;
            else          body.RemoveLast();

            SyncVisuals();
        }

        private void SyncVisuals()
        {
            if (body.Count == 0 || board == null) return;
            transform.position = board.CellToWorld(HeadCell) + Vector3.up * 0.5f;
        }

        private void Die()
        {
            isMoving = false;
            OnDied?.Invoke();
        }
    }
}
