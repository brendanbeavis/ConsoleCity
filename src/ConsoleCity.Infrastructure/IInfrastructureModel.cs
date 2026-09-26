namespace ConsoleCity.Infrastructure;

public interface IInfrastructureModel
{
    InfrastructureSnapshot Snapshot { get; }
}
