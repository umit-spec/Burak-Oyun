using NUnit.Framework;
using UnityEngine;
using BurakOyun.Gameplay;
using BurakOyun.Core;

namespace BurakOyun.Tests
{
    public class GridMovementTests
    {
        [Test]
        public void GetNextPosition_RightDirection_ReturnsNextX()
        {
            var grid = new GridMovement(20, 15);
            Assert.AreEqual(new Vector2Int(6, 5),
                grid.GetNextPosition(new Vector2Int(5, 5), Direction.Right));
        }

        [Test]
        public void GetNextPosition_UpDirection_ReturnsNextY()
        {
            var grid = new GridMovement(20, 15);
            Assert.AreEqual(new Vector2Int(5, 6),
                grid.GetNextPosition(new Vector2Int(5, 5), Direction.Up));
        }

        [Test]
        public void IsWithinBounds_InsideAndOutside()
        {
            var grid = new GridMovement(20, 15);
            Assert.IsTrue(grid.IsWithinBounds(new Vector2Int(0, 0)));
            Assert.IsTrue(grid.IsWithinBounds(new Vector2Int(19, 14)));
            Assert.IsFalse(grid.IsWithinBounds(new Vector2Int(-1, 5)));
            Assert.IsFalse(grid.IsWithinBounds(new Vector2Int(20, 5)));
            Assert.IsFalse(grid.IsWithinBounds(new Vector2Int(5, 15)));
        }

        [Test]
        public void AreOpposite_DetectsOppositePairsOnly()
        {
            Assert.IsTrue(GridMovement.AreOpposite(Direction.Up, Direction.Down));
            Assert.IsTrue(GridMovement.AreOpposite(Direction.Left, Direction.Right));
            Assert.IsFalse(GridMovement.AreOpposite(Direction.Up, Direction.Left));
            Assert.IsFalse(GridMovement.AreOpposite(Direction.Up, Direction.Up));
        }

        [Test]
        public void CellToWorld_CenterCell_IsNearOrigin()
        {
            var grid = new GridMovement(3, 3); // merkez hücre (1,1)
            Vector3 world = grid.CellToWorld(new Vector2Int(1, 1), 1f);
            Assert.AreEqual(0f, world.x, 1e-5f);
            Assert.AreEqual(0f, world.z, 1e-5f);
        }
    }

    public class SnakeBodyTests
    {
        [Test]
        public void Reset_LaysBodyBehindHead()
        {
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3);
            Assert.AreEqual(3, body.Length);
            Assert.AreEqual(new Vector2Int(5, 5), body.HeadPosition);
            Assert.AreEqual(new Vector2Int(3, 5), body.TailPosition); // sağa bakar → gövde solda
        }

        [Test]
        public void AdvanceHead_WithoutRemoveTail_Grows()
        {
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3);
            body.AdvanceHead(new Vector2Int(6, 5));
            Assert.AreEqual(4, body.Length);
            Assert.AreEqual(new Vector2Int(6, 5), body.HeadPosition);
        }

        [Test]
        public void AdvanceHead_WithRemoveTail_KeepsLength()
        {
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3);
            body.AdvanceHead(new Vector2Int(6, 5));
            body.RemoveTail();
            Assert.AreEqual(3, body.Length);
            Assert.AreEqual(new Vector2Int(4, 5), body.TailPosition);
        }

        [Test]
        public void ContainsPosition_SegmentAtPosition_ReturnsTrue()
        {
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3);
            Assert.IsTrue(body.ContainsPosition(new Vector2Int(4, 5)));
            Assert.IsFalse(body.ContainsPosition(new Vector2Int(6, 5)));
        }

        [Test]
        public void ContainsPositionExceptTail_TailCellIsFree()
        {
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3); // kuyruk (3,5)
            Assert.IsFalse(body.ContainsPositionExceptTail(new Vector2Int(3, 5)));
            Assert.IsTrue(body.ContainsPositionExceptTail(new Vector2Int(4, 5)));
        }
    }

    public class CollisionManagerTests
    {
        static (GridMovement grid, SnakeBody body) Rig()
        {
            var grid = new GridMovement(10, 10);
            var body = new SnakeBody();
            body.Reset(new Vector2Int(5, 5), Direction.Right, 3); // (5,5)(4,5)(3,5)
            return (grid, body);
        }

        [Test]
        public void Check_OutsideBounds_ReturnsWall()
        {
            var (grid, body) = Rig();
            Assert.AreEqual(CollisionManager.CollisionType.Wall,
                CollisionManager.Check(grid, body, new Vector2Int(10, 5), false, default));
        }

        [Test]
        public void Check_OwnSegment_ReturnsSelf()
        {
            var (grid, body) = Rig();
            Assert.AreEqual(CollisionManager.CollisionType.Self,
                CollisionManager.Check(grid, body, new Vector2Int(4, 5), false, default));
        }

        [Test]
        public void Check_TailCell_IsSafeWhenNotGrowing()
        {
            var (grid, body) = Rig();
            // Kuyruk (3,5) bu tick'te boşalır → güvenli
            Assert.AreEqual(CollisionManager.CollisionType.None,
                CollisionManager.Check(grid, body, new Vector2Int(3, 5), false, default));
        }

        [Test]
        public void Check_FoodCell_ReturnsFood()
        {
            var (grid, body) = Rig();
            var foodPos = new Vector2Int(6, 5);
            Assert.AreEqual(CollisionManager.CollisionType.Food,
                CollisionManager.Check(grid, body, foodPos, true, foodPos));
        }

        [Test]
        public void Check_EmptyCell_ReturnsNone()
        {
            var (grid, body) = Rig();
            Assert.AreEqual(CollisionManager.CollisionType.None,
                CollisionManager.Check(grid, body, new Vector2Int(6, 5), true, new Vector2Int(0, 0)));
        }
    }

    public class DirectionInputTests
    {
        [Test]
        public void TryGetNextDirection_SkipsOppositeAndSame()
        {
            var go = new GameObject("InputRig");
            var input = go.AddComponent<DirectionInput>();

            input.Enqueue(Direction.Left);  // mevcut yön Right → tersi, atlanır
            input.Enqueue(Direction.Up);    // geçerli

            Assert.IsTrue(input.TryGetNextDirection(Direction.Right, out var next));
            Assert.AreEqual(Direction.Up, next);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Enqueue_CapsAtTwo_NoGhostMoves()
        {
            var go = new GameObject("InputRig");
            var input = go.AddComponent<DirectionInput>();

            input.Enqueue(Direction.Up);
            input.Enqueue(Direction.Left);
            input.Enqueue(Direction.Down); // 3. basış yutulur

            Assert.IsTrue(input.TryGetNextDirection(Direction.Right, out var first));
            Assert.AreEqual(Direction.Up, first);
            Assert.IsTrue(input.TryGetNextDirection(first, out var second));
            Assert.AreEqual(Direction.Left, second);
            Assert.IsFalse(input.TryGetNextDirection(second, out _)); // kuyruk boş

            Object.DestroyImmediate(go);
        }
    }

    public class GameStateManagerTests
    {
        [Test]
        public void SetState_ChangesAndBroadcasts_OnlyOnRealChange()
        {
            var sm = new GameStateManager();
            int fired = 0;
            sm.OnStateChanged += _ => fired++;

            Assert.AreEqual(GameState.Menu, sm.Current);
            sm.SetState(GameState.Playing);
            sm.SetState(GameState.Playing); // aynı durum → event yok
            Assert.AreEqual(GameState.Playing, sm.Current);
            Assert.AreEqual(1, fired);
        }
    }
}
