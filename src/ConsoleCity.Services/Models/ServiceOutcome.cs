namespace ConsoleCity.Services;

public sealed record class ServiceOutcome
{
    public ServiceCapacity Capacity { get; }

    public ServiceCoverage Coverage { get; }

    public ServiceResponse Response { get; }

    public ServiceOutcome(ServiceCapacity capacity, ServiceCoverage coverage, ServiceResponse response)
    {
        ArgumentNullException.ThrowIfNull(capacity);
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(response);
        Capacity = capacity;
        Coverage = coverage;
        Response = response;
    }
}
