using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Yılan sabit hızla ileri (+Z) akar, 3 şerit arasında yumuşak geçiş yapar.
    /// Yanlış harfte kısa süre yavaşlar (ApplySlowdown) — asla durmaz, asla ölmez.
    /// </summary>
    [RequireComponent(typeof(LaneInput))]
    public class SnakeController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private LaneInput input;
        private int currentLane; // -1, 0, +1
        private float slowdownTimer;
        private bool isMoving;

        public bool IsMoving { get => isMoving; set => isMoving = value; }
        public float CurrentSpeed =>
            slowdownTimer > 0f ? config.forwardSpeed * config.slowdownFactor : config.forwardSpeed;

        private void Awake()
        {
            input = GetComponent<LaneInput>();
        }

        private void Update()
        {
            if (!isMoving) return;

            if (slowdownTimer > 0f) slowdownTimer -= Time.deltaTime;

            int dir = input.ConsumeLaneChange();
            if (dir != 0)
                currentLane = Mathf.Clamp(currentLane + dir, -1, 1);

            Vector3 pos = transform.position;
            pos.z += CurrentSpeed * Time.deltaTime;
            float targetX = currentLane * config.laneWidth;
            pos.x = Mathf.MoveTowards(pos.x, targetX, config.laneChangeSpeed * Time.deltaTime);
            transform.position = pos;
        }

        public void ApplySlowdown() => slowdownTimer = config.slowdownDuration;

        public void ResetToStart()
        {
            currentLane = 0;
            slowdownTimer = 0f;
            transform.position = new Vector3(0f, transform.position.y, 0f);
        }
    }
}
