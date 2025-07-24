namespace StockWise.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int ProductId { get; set; }
        public int Quantity {  get; set; }
        public string? Comment {  get; set; }
        public Product Product { get; set; }
    }
}
