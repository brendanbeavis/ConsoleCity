using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class EconomicTransfer
{
    public EconomicTransferType TransferType { get; }

    public Money Amount { get; }

    public OrganizationId? FromBusinessId { get; }

    public OrganizationId? ToBusinessId { get; }

    public PersonId? ToPersonId { get; }

    public HouseholdId? ToHouseholdId { get; }

    public string Reason { get; }

    public EconomicTransfer(
        EconomicTransferType transferType,
        Money amount,
        string reason,
        OrganizationId? fromBusinessId = null,
        OrganizationId? toBusinessId = null,
        PersonId? toPersonId = null,
        HouseholdId? toHouseholdId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Transfer reason cannot be empty.", nameof(reason));
        }

        TransferType = transferType;
        Amount = amount;
        Reason = reason.Trim();
        FromBusinessId = fromBusinessId;
        ToBusinessId = toBusinessId;
        ToPersonId = toPersonId;
        ToHouseholdId = toHouseholdId;
    }
}
