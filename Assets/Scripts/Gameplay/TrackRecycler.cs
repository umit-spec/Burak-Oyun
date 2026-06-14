using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// Izgara oyununa geçildi; bu bileşen artık kullanılmıyor.
    public class TrackRecycler : MonoBehaviour
    {
#pragma warning disable CS0414
        [SerializeField] private Transform snake;
        [SerializeField] private Transform[] tiles;
        [SerializeField] private float tileLength = 30f;
#pragma warning restore CS0414

        private void Update() { }
    }
}
