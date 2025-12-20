using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Application.Contracts.CompanyProductDtos
{
    public enum CompanyProductSortBy
    {
        Stock,
        Price,
        CompanyName,
        CategoryName
    }
    public class CompanyProductsAvailableQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int? Stock { get; set; }

        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }

        public CompanyProductSortBy SortedBy { get; set; }
            = CompanyProductSortBy.Stock;
        public SortDir SortDir { get; set; } = SortDir.Asc;
    }
}
