namespace ConsoleCity.Core;

public readonly record struct GameDateTime
{
    public const int HoursPerDay = 24;
    public const int DaysPerWeek = 7;
    public const int MonthsPerYear = 12;
    public const int DaysPerMonth = 30;
    public const int HoursPerMonth = HoursPerDay * DaysPerMonth;
    public const int HoursPerYear = HoursPerMonth * MonthsPerYear;

    public int Year { get; }
    public int Month { get; }
    public int Day { get; }
    public int Hour { get; }

    public GameDateTime(int year, int month, int day, int hour)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be greater than zero.");
        }

        if (month is < 1 or > MonthsPerYear)
        {
            throw new ArgumentOutOfRangeException(nameof(month), $"Month must be between 1 and {MonthsPerYear}.");
        }

        if (day is < 1 or > DaysPerMonth)
        {
            throw new ArgumentOutOfRangeException(nameof(day), $"Day must be between 1 and {DaysPerMonth}.");
        }

        if (hour is < 0 or >= HoursPerDay)
        {
            throw new ArgumentOutOfRangeException(nameof(hour), $"Hour must be between 0 and {HoursPerDay - 1}.");
        }

        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
    }

    public static GameDateTime FromSimulationTime(SimulationTime time)
    {
        var totalHours = time.Tick;
        var year = (int)(totalHours / HoursPerYear) + 1;
        var hourOfYear = (int)(totalHours % HoursPerYear);
        var month = hourOfYear / HoursPerMonth + 1;
        var hourOfMonth = hourOfYear % HoursPerMonth;
        var day = hourOfMonth / HoursPerDay + 1;
        var hour = hourOfMonth % HoursPerDay;

        return new GameDateTime(year, month, day, hour);
    }

    public SimulationTime ToSimulationTime()
    {
        var hours = ((long)Year - 1) * HoursPerYear
            + ((long)Month - 1) * HoursPerMonth
            + ((long)Day - 1) * HoursPerDay
            + Hour;

        return new SimulationTime(hours);
    }

    public GameDateTime AdvanceHours(long hours) => FromSimulationTime(ToSimulationTime().Advance(hours));

    public override string ToString() => $"{Year:D4}-{Month:D2}-{Day:D2} {Hour:D2}:00";
}
