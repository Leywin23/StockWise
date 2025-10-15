namespace StockWise.Models
{
    public class RevokedToken
    {
        public long Id { get; set; }
        public string Jti { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public string? Reason {  get; set; }
        public string? UserId {  get; set; }
        public DateTime RevokedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
