using ConsoleCity.Core;

namespace ConsoleCity.Infrastructure;

public interface IInfrastructureModel
{
    InfrastructureSnapshot Snapshot { get; }

    InfrastructureAdvanceResult Advance(SimulationTime currentTime);

    void SetNodeDemand(string nodeId, decimal demand);

    void SetNodeOperational(string nodeId, bool isOperational);

    void SetEdgeOperational(string edgeId, bool isOperational);

    void SetConnections(IReadOnlyList<UtilityConnection> connections);

    void RegisterConnection(string consumerId, string nodeId);

    void UnregisterConnection(string consumerId);

    UtilityConnection? GetConnection(string consumerId);

    IReadOnlyList<UtilityConnection> GetConnectionsForNode(string nodeId);

    string? GetNodeIdForConsumer(string consumerId);

    IReadOnlyList<string> GetConsumersForNode(string nodeId);
}