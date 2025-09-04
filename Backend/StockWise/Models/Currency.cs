namespace StockWise.Models
{
    public class Currency
    {
        public string Code { get; private set; } = default!;

        private Currency() { }
        public Currency(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code), "Currency code can't be empty");

            var normalized = code.Trim().ToUpperInvariant();
            if (normalized.Length != 3 || !normalized.All(ch => ch is >= 'A' and <= 'Z'))
                throw new ArgumentException("Currency code must be 3 letters (ISO 4217)", nameof(code));

            Code = normalized;
        }

        public override string ToString() => $"{Code}";
    }
}
