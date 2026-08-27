namespace Domain.ValueObjects
{
    public record Money(decimal Amount, string Currency = "UAH")
    {
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency) throw new InvalidOperationException("Currency mismatch");
            return a with { Amount = a.Amount + b.Amount };
        }

        public Money ApplyDiscount(decimal percent) => this with { Amount = Amount * (1 - percent) };
        public Money ApplySurcharge(decimal percent) => this with { Amount = Amount * (1 + percent) };
    }
}
