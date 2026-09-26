namespace ConsoleCity.Core;

public interface IWorldEntity<out TId> : IEntity<TId>
{
    SimulationTick CreatedAt { get; }

    GridPosition? Location { get; }
}
