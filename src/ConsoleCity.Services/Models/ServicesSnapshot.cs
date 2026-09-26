using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServicesSnapshot
{
    public SimulationTime CapturedAt { get; }

    public IReadOnlyList<ServiceProvider> Providers { get; }

    public IReadOnlyList<ServiceRequest> Requests { get; }

    public IReadOnlyList<ServiceResponse> Responses { get; }

    public ServicesSnapshot(SimulationTime capturedAt, IReadOnlyList<ServiceProvider> providers, IReadOnlyList<ServiceRequest> requests, IReadOnlyList<ServiceResponse> responses)
    {
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(responses);
        CapturedAt = capturedAt;
        Providers = providers;
        Requests = requests;
        Responses = responses;
    }

    public static ServicesSnapshot Empty { get; } = new(new SimulationTime(0), Array.Empty<ServiceProvider>(), Array.Empty<ServiceRequest>(), Array.Empty<ServiceResponse>());
}
