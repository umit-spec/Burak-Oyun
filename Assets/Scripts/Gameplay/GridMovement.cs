using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// <summary>4 hareket yönü.</summary>
    public enum Direction { Up, Down, Left, Right }

    /// <summary>
    /// Saf C# ızgara çekirdeği — Unity sahnesiz, unit-test edilebilir.
    /// (0,0) sol-alt köşe; X sağa, Y yukarı (dünya XZ düzlemine birebir oturur).
    /// </summary>
    public class GridMovement
    {
        public Vector2Int GridSize { get; }

        public GridMovement(int width, int height)
        {
            GridSize = new Vector2Int(width, height);
        }

        public static Vector2Int ToVector(Direction dir) => dir switch
        {
            Direction.Up => new Vector2Int(0, 1),
            Direction.Down => new Vector2Int(0, -1),
            Direction.Left => new Vector2Int(-1, 0),
            _ => new Vector2Int(1, 0),
        };

        /// <summary>Ters yönler (Up/Down, Left/Right) — geri dönüş anında kendine çarpma demektir.</summary>
        public static bool AreOpposite(Direction a, Direction b) =>
            ToVector(a) + ToVector(b) == Vector2Int.zero;

        public Vector2Int GetNextPosition(Vector2Int current, Direction dir) =>
            current + ToVector(dir);

        public bool IsWithinBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < GridSize.x && pos.y >= 0 && pos.y < GridSize.y;

        /// <summary>Hücreyi dünya konumuna çevirir (tahta orijinde ortalanır; ızgara Y'si dünya Z'sine gider).</summary>
        public Vector3 CellToWorld(Vector2Int cell, float cellSize, float y = 0f) =>
            new Vector3(
                (cell.x - (GridSize.x - 1) * 0.5f) * cellSize,
                y,
                (cell.y - (GridSize.y - 1) * 0.5f) * cellSize);
    }
}
