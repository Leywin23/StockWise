using StockWise.Application.Contracts.CompanyDtos;
using StockWise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Application.Contracts.CompanyProductDtos
{
    public class CompanyProductDtoWithCompany
    {
        public string CompanyProductName { get; set; }
        public string EAN { get; set; }
        public string CategoryName { get; set; }
        public string? Image { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }
        public int Stock { get; set; } = 0;
        public CompanyDto Company { get; set; }
    }
}
