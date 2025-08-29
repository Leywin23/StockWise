namespace StockWise.Models
{
    public class Currency
    {
        public string CurrencyCode { get; }
        public Currency(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                throw new ArgumentNullException("Currency code can't be empty");

            CurrencyCode = currencyCode;
        }

        public override string ToString() => $"{CurrencyCode}";
    }
}
