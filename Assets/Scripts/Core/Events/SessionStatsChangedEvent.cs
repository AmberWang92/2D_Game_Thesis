namespace TopDownShooter.Core.Events
{
    public readonly struct SessionStatsChangedEvent
    {
        public float SurvivalTime { get; }
        public int Kills { get; }
        public int Score { get; }

        public SessionStatsChangedEvent(float survivalTime, int kills, int score)
        {
            SurvivalTime = survivalTime;
            Kills = kills;
            Score = score;
        }
    }
}
