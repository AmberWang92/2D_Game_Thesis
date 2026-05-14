namespace TopDownShooter.Core.Time
{
    public interface ITimeProvider
    {
        float Time { get; }
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
    }
}
