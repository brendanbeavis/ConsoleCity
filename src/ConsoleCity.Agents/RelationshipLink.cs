using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record RelationshipLink
{
    public PersonId PersonId { get; }

    public RelationshipType RelationshipType { get; }

    public double Strength { get; }

    public RelationshipLink(PersonId personId, RelationshipType relationshipType, double strength)
    {
        if (!double.IsFinite(strength) || strength is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "Relationship strength must be a finite fraction between 0 and 1.");
        }

        PersonId = personId;
        RelationshipType = relationshipType;
        Strength = strength;
    }
}
