namespace ConsoleCity.Agents;

public sealed record SkillRating
{
    public string Name { get; }

    public double Level { get; }

    public SkillRating(string name, double level)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Skill name cannot be empty.", nameof(name));
        }

        if (!double.IsFinite(level) || level is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Skill level must be a finite fraction between 0 and 1.");
        }

        Name = name.Trim();
        Level = level;
    }
}
