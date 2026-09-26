namespace ConsoleCity.Core;

public readonly record struct EntityId
{
    public Guid Value { get; }

    public EntityId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Entity identifiers cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static EntityId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
