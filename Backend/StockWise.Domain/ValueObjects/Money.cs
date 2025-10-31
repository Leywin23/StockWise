namespace StockWise.Models
{
    public class Money
    {
        public decimal Amount { get; private set; }
        public Currency Currency { get; private set; } = default!;

        private Money() { }

        public Money(decimal amount, Currency currency)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException("Amount must be >= 0");

            Amount = amount;
            Currency = currency;
        }

        public static Money Of(decimal amount, string currencyCode)
        {
            var Currency = new Currency(currencyCode);
            return new Money(amount, Currency);
        }

        public override string ToString() => $"{Amount}, {Currency}";
    }
}
