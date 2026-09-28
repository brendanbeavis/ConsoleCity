using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class ResourceDepositModel
{
    public ResourceType ResourceType { get; }

    public GridPosition Location { get; }

    public Quantity Quantity { get; }

    public double Quality { get; }

    public Quantity ExtractionRate { get; }

    public SimulationTime CreatedAt { get; }

    public ResourceDepositModel(
        ResourceType resourceType,
        GridPosition location,
        Quantity quantity,
        double quality,
        Quantity extractionRate,
        SimulationTime createdAt)
    {
        if (!double.IsFinite(quality) || quality is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), "Resource deposit quality must be a finite fraction between 0 and 1.");
        }

        ResourceType = resourceType;
        Location = location;
        Quantity = quantity;
        Quality = quality;
        ExtractionRate = extractionRate;
        CreatedAt = createdAt;
    }
}
