namespace StockWise.Models
{
    public class PageResult<T>
    {
        public int PageSize {  get; set; }
        public int Page {  get; set; }
        public int TotalCount {  get; set; }
        public SortDir SortDir { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public List<T> Items { get; set; } = new();
    }
}
