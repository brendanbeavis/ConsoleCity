using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class ProductionRecipe
{
    public string RecipeKey { get; }

    public IReadOnlyList<InventoryLine> Inputs { get; }

    public IReadOnlyList<InventoryLine> Outputs { get; }

    public Money OperatingCost { get; }

    public ProductionRecipe(
        string recipeKey,
        IReadOnlyList<InventoryLine> inputs,
        IReadOnlyList<InventoryLine> outputs,
        Money? operatingCost = null)
    {
        if (string.IsNullOrWhiteSpace(recipeKey))
        {
            throw new ArgumentException("Recipe key cannot be empty.", nameof(recipeKey));
        }

        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(outputs);

        if (inputs.Count == 0)
        {
            throw new ArgumentException("A recipe must have at least one input.", nameof(inputs));
        }

        if (outputs.Count == 0)
        {
            throw new ArgumentException("A recipe must have at least one output.", nameof(outputs));
        }

        RecipeKey = recipeKey.Trim();
        Inputs = inputs;
        Outputs = outputs;
        OperatingCost = operatingCost ?? Money.Zero;
    }
}
