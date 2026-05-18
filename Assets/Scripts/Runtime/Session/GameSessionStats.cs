namespace TopDownShooter.Runtime.Session
{
    public readonly struct GameSessionStats
    {
        public float SurvivalTime { get; }
        public int Kills { get; }
        public int Score { get; }

        public GameSessionStats(float survivalTime, int kills, int score)
        {
            SurvivalTime = survivalTime;
            Kills = kills;
            Score = score;
        }
    }
}
