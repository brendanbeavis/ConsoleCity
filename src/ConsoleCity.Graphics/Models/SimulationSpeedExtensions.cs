namespace ConsoleCity.Graphics.Models;

public static class SimulationSpeedExtensions
{
    private static readonly SimulationSpeed[] OrderedSpeeds =
    [
        SimulationSpeed.OneX,
        SimulationSpeed.TwoX,
        SimulationSpeed.FiveX,
        SimulationSpeed.TenX,
        SimulationSpeed.FiftyX
    ];

    public static int GetTicksPerSecond(this SimulationSpeed speed)
        => speed switch
        {
            SimulationSpeed.OneX => 1,
            SimulationSpeed.TwoX => 2,
            SimulationSpeed.FiveX => 5,
            SimulationSpeed.TenX => 10,
            SimulationSpeed.FiftyX => 50,
            _ => 1
        };

    public static string ToDisplayLabel(this SimulationSpeed speed)
        => speed switch
        {
            SimulationSpeed.OneX => "1x",
            SimulationSpeed.TwoX => "2x",
            SimulationSpeed.FiveX => "5x",
            SimulationSpeed.TenX => "10x",
            SimulationSpeed.FiftyX => "50x",
            _ => "1x"
        };

    public static SimulationSpeed Faster(this SimulationSpeed speed)
    {
        var index = Array.IndexOf(OrderedSpeeds, speed);
        return index >= OrderedSpeeds.Length - 1 ? OrderedSpeeds[^1] : OrderedSpeeds[index + 1];
    }

    public static SimulationSpeed Slower(this SimulationSpeed speed)
    {
        var index = Array.IndexOf(OrderedSpeeds, speed);
        return index <= 0 ? OrderedSpeeds[0] : OrderedSpeeds[index - 1];
    }

    public static IReadOnlyList<SimulationSpeed> GetOrderedSpeeds() => OrderedSpeeds;
}
