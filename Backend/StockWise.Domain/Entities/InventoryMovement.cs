namespace StockWise.Models
{
    public enum MovementType
    {
        Inbound,
        Outbound,
        Adjustment
    }
    public class InventoryMovement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public MovementType Type { get; set; }
        public int CompanyProductId { get; set; }
        public int Quantity {  get; set; }
        public string? Comment {  get; set; }
        public CompanyProduct CompanyProduct { get; set; }
    }
}
