namespace StockWise.Dtos
{
    public class InventoryMovementDto
    {
        public int CompanyProductId { get; set; } 
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public string? Comment { get; set; }
    }
}

