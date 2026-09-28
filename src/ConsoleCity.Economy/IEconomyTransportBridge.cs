using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public interface IEconomyTransportBridge
{
    void RequestTransport(GoodsTransportRequest request);
}
