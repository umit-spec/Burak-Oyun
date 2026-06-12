using System;

namespace BurakOyun.Core
{
    public enum GameState { Menu, Playing, GameOver }

    /// <summary>
    /// Basit durum makinesi. MonoBehaviour değil — GameManager sahiplenir, unit-test edilebilir.
    /// </summary>
    public class GameStateManager
    {
        public GameState Current { get; private set; } = GameState.Menu;
        public event Action<GameState> OnStateChanged;

        public void SetState(GameState state)
        {
            if (state == Current) return;
            Current = state;
            OnStateChanged?.Invoke(state);
        }
    }
}
