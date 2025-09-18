namespace StockWise.Models
{
    public class CompanyQueryParams
    {
        public int Page {  get; set; }
        public int PageSize {  get; set; } = 10;
        public bool WithOrdersAsBuyer {  get; set; } = false;
        public bool WithOrdersAsSeller { get; set; } = false;
        public bool WithCompanyUsers { get; set; } = false;
        public bool WithCompanyProducts {  get; set; } = false;
        public string? SortedBy { get; set; }
        public SortDir SortDir { get; set; }
    }
}
