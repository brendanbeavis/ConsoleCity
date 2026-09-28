using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Economy;

public sealed record class BusinessModel
{
    public OrganizationId Id { get; }

    public string Name { get; }

    public EconomicActorType ActorType { get; }

    public BusinessLifecycleState State { get; }

    public SimulationTime FoundedAt { get; }

    public BuildingId? BuildingId { get; }

    public GridPosition? Location { get; }

    public Money Cash { get; }

    public Money Revenue { get; }

    public Money Expenses { get; }

    public Money WagePerEmployee { get; }

    public IReadOnlyList<PersonId> Employees { get; }

    public IReadOnlyList<InventoryLine> Inventory { get; }

    public IReadOnlyList<string> RecipeKeys { get; }

    public BusinessModel(
        OrganizationId id,
        string name,
        EconomicActorType actorType,
        BusinessLifecycleState state,
        SimulationTime foundedAt,
        BuildingId? buildingId,
        GridPosition? location,
        Money cash,
        Money revenue,
        Money expenses,
        Money wagePerEmployee,
        IReadOnlyList<PersonId> employees,
        IReadOnlyList<InventoryLine> inventory,
        IReadOnlyList<string> recipeKeys)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Business name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(employees);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(recipeKeys);

        Name = name.Trim();
        Id = id;
        ActorType = actorType;
        State = state;
        FoundedAt = foundedAt;
        BuildingId = buildingId;
        Location = location;
        Cash = cash;
        Revenue = revenue;
        Expenses = expenses;
        WagePerEmployee = wagePerEmployee;
        Employees = employees;
        Inventory = inventory;
        RecipeKeys = recipeKeys;
    }

    public bool HasEmployee(PersonId personId) => Employees.Contains(personId);
}
