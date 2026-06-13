using UnityEngine;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Saf C# çarpışma kontrolü — duvar / kendine / yem tek noktadan. Unit-test edilebilir.
    /// </summary>
    public static class CollisionManager
    {
        public enum CollisionType { None, Food, Wall, Self }

        public static CollisionType Check(GridMovement grid, SnakeBody body,
            Vector2Int nextPos, bool hasFood, Vector2Int foodPos)
        {
            if (!grid.IsWithinBounds(nextPos))
                return CollisionType.Wall;

            bool willGrow = hasFood && nextPos == foodPos;

            // Büyümüyorsak kuyruk bu tick'te boşalır → kuyruk hücresi güvenli.
            // Büyüyorsak kuyruk yerinde kalır → tüm gövde ölümcül.
            bool hitsSelf = willGrow
                ? body.ContainsPosition(nextPos)
                : body.ContainsPositionExceptTail(nextPos);
            if (hitsSelf)
                return CollisionType.Self;

            return willGrow ? CollisionType.Food : CollisionType.None;
        }
    }
}
