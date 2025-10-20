using AutoMapper;
using StockWise.Dtos.AccountDtos;
using StockWise.Dtos.CompanyDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.OrderDtos;
using StockWise.Dtos.ProductDtos;
using StockWise.Models;

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

            CreateMap<AppUser, CompanyUserDto>();
            CreateMap<Company, CompanyMiniDto>();
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalPrice.Amount))
                .ForMember(d => d.TotalCurrencyCode, o => o.MapFrom(s => s.TotalPrice.Currency.Code))
                .ForMember(d => d.Counterparty, o => o.Ignore());

            CreateMap<Company, AdvancedCompanyDto>()
                .ForMember(d => d.OrdersAsBuyer, o => o.Ignore())
                .ForMember(d => d.OrdersAsSeller, o => o.Ignore())
                .ForMember(d => d.Users, o => o.Ignore());

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.ProductsWithQuantity));

            CreateMap<OrderProduct, OrderProductDto>()
                .ForMember(dest => dest.CompanyProductId, opt => opt.MapFrom(src => src.CompanyProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.CompanyProduct.CompanyProductName))
                .ForMember(dest => dest.EAN, opt => opt.MapFrom(src => src.CompanyProduct.EAN))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.CompanyProduct.Price.Amount));


        }
    }
}
