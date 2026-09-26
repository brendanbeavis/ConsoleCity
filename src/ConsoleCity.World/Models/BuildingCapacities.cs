namespace ConsoleCity.World;

public readonly record struct BuildingCapacities
{
    public int Residents { get; }

    public int Jobs { get; }

    public int Customers { get; }

    public int ServiceThroughput { get; }

    public int Storage { get; }

    public int Production { get; }

    public BuildingCapacities(int residents, int jobs, int customers, int serviceThroughput, int storage, int production)
    {
        if (residents < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(residents));
        }

        if (jobs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(jobs));
        }

        if (customers < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customers));
        }

        if (serviceThroughput < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(serviceThroughput));
        }

        if (storage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(storage));
        }

        if (production < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(production));
        }

        Residents = residents;
        Jobs = jobs;
        Customers = customers;
        ServiceThroughput = serviceThroughput;
        Storage = storage;
        Production = production;
    }

    public static BuildingCapacities Empty => new(0, 0, 0, 0, 0, 0);
}
