using TopDownShooter.Core.FSM;

namespace TopDownShooter.Game.States
{
    /// <summary>Terminal state. Spawner gates itself on <see cref="GameManager.IsRunning"/>;
    /// GameOverPanel listens to the gameOverChannel raised on entry.</summary>
    public sealed class GameOverState : IState<GameManager>
    {
        public void Enter(GameManager c) { }
        public void Tick(GameManager c, float dt) { }
        public void FixedTick(GameManager c, float fdt) { }
        public void Exit(GameManager c) { }
    }
}
