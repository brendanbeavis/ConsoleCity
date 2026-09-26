namespace ConsoleCity.Agents;

public readonly record struct AgentAttributeProfile
{
    public double Health { get; }

    public double Energy { get; }

    public double Sociability { get; }

    public double Diligence { get; }

    public AgentAttributeProfile(double health, double energy, double sociability, double diligence)
    {
        ValidateFraction(health, nameof(health));
        ValidateFraction(energy, nameof(energy));
        ValidateFraction(sociability, nameof(sociability));
        ValidateFraction(diligence, nameof(diligence));

        Health = health;
        Energy = energy;
        Sociability = sociability;
        Diligence = diligence;
    }

    public static AgentAttributeProfile Default => new(1d, 1d, 0.5d, 0.5d);

    private static void ValidateFraction(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Attributes must be finite fractions between 0 and 1.");
        }
    }
}
