namespace StockWise.Models
{
    public class CompanyProduct
    {
        public int CompanyProductId { get; set; }
        public string CompanyProductName { get; set; }
        public string EAN { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public decimal ShoppingPrice { get; set; } = 0;
        public decimal SellingPrice { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public int CompanyId {  get; set; }
        public Company Company { get; set; }
        public bool IsAvailableForOrder { get; set; }

        public ICollection<InventoryMovement> InventoryMovements { get; set; }

    }
}
