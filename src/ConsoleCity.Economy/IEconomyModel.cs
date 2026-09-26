namespace ConsoleCity.Economy;

public interface IEconomyModel
{
    EconomySnapshot Snapshot { get; }
}
