using AutoMapper;
using StockWise.Models;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.ProductDtos;

namespace StockWise.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCompanyProductDto, CompanyProduct>()
                .ForMember(d => d.CompanyProductName, opt => opt.MapFrom(s => s.CompanyProductName))

                .ForPath(d => d.Price.Amount, opt => opt.MapFrom(s => s.Price))
                .ForPath(d => d.Price.Currency.Code, opt => opt.MapFrom(s => s.Currency.Code))

                .ForMember(d => d.CompanyId, opt => opt.Ignore())
                .ForMember(d => d.Company, opt => opt.Ignore())
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore());

            CreateMap<CompanyProduct, CompanyProductDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));
            CreateMap<Company, CompanyDto>();
            CreateMap(typeof(PageResult<>), typeof(PageResult<>));
            CreateMap<CompanyProductDto, CompanyProduct>();

            CreateMap<Product, ProductDto>();
            CreateMap<CompanyProduct, CreateCompanyProductDto>();
        }
    }
}
