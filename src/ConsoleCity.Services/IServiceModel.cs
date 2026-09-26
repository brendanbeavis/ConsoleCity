using ConsoleCity.Core;

namespace ConsoleCity.Services;

public interface IServiceModel
{
    ServicesSnapshot Snapshot { get; }

    void RegisterProvider(ServiceProvider provider);

    ServiceResponse SubmitRequest(ServiceRequest request);

    ServicesSnapshot Advance(SimulationTime currentTime);
}
