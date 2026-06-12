using UnityEngine;

namespace BurakOyun.Gameplay
{
    public class SnakeBob : MonoBehaviour
    {
        [SerializeField] private float bobAmount = 0.15f;
        [SerializeField] private float bobSpeed = 3f;

        private float baseY;

        private void Start() => baseY = transform.localPosition.y;

        private void Update()
        {
            var pos = transform.localPosition;
            pos.y = baseY + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = pos;
        }
    }
}
