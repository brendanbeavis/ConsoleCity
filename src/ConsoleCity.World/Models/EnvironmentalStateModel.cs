namespace ConsoleCity.World;

public readonly record struct EnvironmentalStateModel
{
    public double Pollution { get; }

    public double WaterQuality { get; }

    public double GreenSpace { get; }

    public double LandDegradation { get; }

    public EnvironmentalStateModel(double pollution, double waterQuality, double greenSpace, double landDegradation)
    {
        ValidateFraction(pollution, nameof(pollution));
        ValidateFraction(waterQuality, nameof(waterQuality));
        ValidateFraction(greenSpace, nameof(greenSpace));
        ValidateFraction(landDegradation, nameof(landDegradation));

        Pollution = pollution;
        WaterQuality = waterQuality;
        GreenSpace = greenSpace;
        LandDegradation = landDegradation;
    }

    public static EnvironmentalStateModel Neutral => new(0d, 1d, 0.5d, 0d);

    private static void ValidateFraction(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Environmental values must be finite fractions between 0 and 1.");
        }
    }
}
