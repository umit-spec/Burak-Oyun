using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Sonsuz düz yol: yılanın gerisinde kalan zemin tile'ı en öne taşınır.
    /// 3 tile yeterli (biri altında, biri önde, biri arkada).
    /// </summary>
    public class TrackRecycler : MonoBehaviour
    {
        [SerializeField] private Transform snake;
        [SerializeField] private Transform[] tiles;
        [SerializeField] private float tileLength = 30f;

        private void Update()
        {
            foreach (var tile in tiles)
            {
                if (snake.position.z - tile.position.z > tileLength)
                    tile.position += Vector3.forward * (tileLength * tiles.Length);
            }
        }
    }
}
