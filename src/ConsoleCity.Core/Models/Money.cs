namespace ConsoleCity.Core;

public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money Zero => new(0m);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);

    public static Money operator *(Money left, decimal multiplier) => new(left.Amount * multiplier);

    public static Money operator /(Money left, decimal divisor) => new(left.Amount / divisor);

    public override string ToString() => Amount.ToString();
}
