namespace TopDownShooter.Core.Events
{
    public readonly struct WaveCompletedEvent
    {
        public int WaveIndex { get; }

        public WaveCompletedEvent(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }
}
