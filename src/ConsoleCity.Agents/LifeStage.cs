namespace ConsoleCity.Agents;

public enum LifeStage
{
    Child,
    Teenager,
    YoungAdult,
    Adult,
    Senior
}

public static class LifeStageExtensions
{
    public static LifeStage FromAge(int age)
    {
        if (age < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(age));
        }

        if (age < 13)
        {
            return LifeStage.Child;
        }

        if (age < 18)
        {
            return LifeStage.Teenager;
        }

        if (age < 25)
        {
            return LifeStage.YoungAdult;
        }

        if (age < 65)
        {
            return LifeStage.Adult;
        }

        return LifeStage.Senior;
    }
}
