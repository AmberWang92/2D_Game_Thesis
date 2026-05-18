using TopDownShooter.Core.FSM;

namespace TopDownShooter.Game.States
{
    /// <summary>Active gameplay: advances the run clock; SpawnDirector / Score / HUD react via channels.</summary>
    public sealed class GameRunningState : IState<GameManager>
    {
        public void Enter(GameManager c) { }
        public void Tick(GameManager c, float dt) => c.AdvanceTime(dt);
        public void FixedTick(GameManager c, float fdt) { }
        public void Exit(GameManager c) { }
    }
}
