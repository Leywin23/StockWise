namespace StockWise.Dtos
{
    public class InventoryMovementDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Comment { get; set; }
    }
}

