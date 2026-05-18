namespace TopDownShooter.Core.Events
{
    public readonly struct GameOverEvent
    {
        public float SurvivalTime { get; }
        public int Kills { get; }
        public int Score { get; }

        public GameOverEvent(float survivalTime, int kills, int score)
        {
            SurvivalTime = survivalTime;
            Kills = kills;
            Score = score;
        }
    }
}
