using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    public class GridBoard : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        public int Width  => config.gridWidth;
        public int Height => config.gridHeight;
        public float Cell => config.cellSize;

        public Vector3 BoardCenter =>
            new(0f, 0f, (config.gridHeight - 1) * 0.5f * config.cellSize);

        public Vector3 CellToWorld(Vector2Int cell) =>
            new(
                (cell.x - (config.gridWidth - 1) * 0.5f) * config.cellSize,
                0f,
                cell.y * config.cellSize
            );

        public bool InBounds(Vector2Int cell) =>
            cell.x >= 0 && cell.x < config.gridWidth &&
            cell.y >= 0 && cell.y < config.gridHeight;

        public Vector2Int RandomEmpty(HashSet<Vector2Int> occupied)
        {
            Vector2Int cell;
            int tries = 0;
            do {
                cell = new Vector2Int(
                    Random.Range(0, config.gridWidth),
                    Random.Range(0, config.gridHeight));
                if (++tries > 500) break;
            } while (occupied.Contains(cell));
            return cell;
        }
    }
}
