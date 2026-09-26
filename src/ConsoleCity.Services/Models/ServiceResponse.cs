using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceResponse
{
    public Guid RequestId { get; }

    public ServiceType ServiceType { get; }

    public bool Success { get; }

    public int Provided { get; }

    public SimulationTime RespondedAt { get; }

    public string Message { get; }

    public ServiceCoverage Coverage { get; }

    public ServiceResponse(Guid requestId, ServiceType serviceType, bool success, int provided, SimulationTime respondedAt, string message, ServiceCoverage coverage)
    {
        if (provided < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(provided));
        }

        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(coverage);
        RequestId = requestId;
        ServiceType = serviceType;
        Success = success;
        Provided = provided;
        RespondedAt = respondedAt;
        Message = message;
        Coverage = coverage;
    }
}
