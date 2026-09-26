namespace ConsoleCity.Transport;

public sealed record class PedestrianProfile
{
    public double WalkingSpeedKph { get; }

    public double AccessibilityBias { get; }

    public PedestrianProfile(double walkingSpeedKph, double accessibilityBias)
    {
        if (!double.IsFinite(walkingSpeedKph) || walkingSpeedKph <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(walkingSpeedKph));
        }

        if (!double.IsFinite(accessibilityBias))
        {
            throw new ArgumentOutOfRangeException(nameof(accessibilityBias));
        }

        WalkingSpeedKph = walkingSpeedKph;
        AccessibilityBias = accessibilityBias;
    }
}
