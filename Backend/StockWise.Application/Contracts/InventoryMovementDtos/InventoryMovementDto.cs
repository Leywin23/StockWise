using StockWise.Models;

namespace StockWise.Application.Contracts.InventoryMovementDtos
{
    public class InventoryMovementDto
    {
        public int CompanyProductId { get; set; }
        public DateTime Date { get; set; }
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public string? Comment { get; set; }
    }
}

