namespace StockWise.Models
{
    public class Money
    {
        public decimal amount { get; }
        public Currency currency { get; }

        public Money(decimal Amount, Currency Currency)
        {
            if (Amount <= 0) throw new ArgumentOutOfRangeException("Amount must be >= 0");

            amount = Amount;
            currency = Currency;
        }

        public static Money Of(decimal Amount, string CurrencyCode)
        {
            var currency = new Currency(CurrencyCode);
            return new Money(Amount, currency);
        }

        public override string ToString() => $"{amount}, {currency}";
    }
}
