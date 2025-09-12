namespace StockWise.Models
{
    public class CompanyProductQueryParams
    {
        public int Page {  get; set; }
        public int PageSize {  get; set; }
        public int Stock {  get; set; }
        public bool IsAvailableForOrder { get; set; }
        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }

        public string? SortedBy { get; set; } = "stock";
        public SortDir SortDir { get; set; }

    }
}
