using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Yılan sabit hızla ileri akar; her doğru harfte hız biraz artar (zorluk eğrisi).
    /// Yanlış harfte kısa süre yavaşlar — asla durmaz, asla ölmez.
    /// </summary>
    [RequireComponent(typeof(LaneInput))]
    public class SnakeController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private LaneInput input;
        private int currentLane;
        private float slowdownTimer;
        private bool isMoving;
        private Vector3 spawnPosition;
        private float speedBoost; // her doğru harfte 0.3 artar, max %60 boost

        public bool IsMoving { get => isMoving; set => isMoving = value; }
        public float CurrentSpeed
        {
            get
            {
                float speed = config.forwardSpeed + speedBoost;
                return slowdownTimer > 0f ? speed * config.slowdownFactor : speed;
            }
        }

        private void Awake()
        {
            input = GetComponent<LaneInput>();
            spawnPosition = transform.position;
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

        public void AddSpeedBoost()
        {
            float maxBoost = config.forwardSpeed * 0.6f;
            speedBoost = Mathf.Min(speedBoost + 0.3f, maxBoost);
        }

        public void ResetToStart()
        {
            currentLane = 0;
            slowdownTimer = 0f;
            speedBoost = 0f;
            transform.position = new Vector3(0f, spawnPosition.y, spawnPosition.z);
        }
    }
}
