using AutoMapper;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.AccountDtos;
using StockWise.Application.Contracts.CompanyDtos;
using StockWise.Application.Contracts.CompanyProductDtos;
using StockWise.Application.Contracts.InventoryMovementDtos;
using StockWise.Application.Contracts.OrderDtos;
using StockWise.Application.Contracts.ProductDtos;
using StockWise.Models;

namespace StockWise.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCompanyProductDto, CompanyProduct>()
                .ForMember(d => d.CompanyProductName, opt => opt.MapFrom(s => s.CompanyProductName))

                .ForPath(d => d.Price.Amount, opt => opt.MapFrom(s => s.Price))
                .ForPath(d => d.Price.Currency.Code, opt => opt.MapFrom(s => s.Currency))

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

            CreateMap<InventoryMovement, InventoryMovementDto>();

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


            CreateMap<CreateProductDto, Product>()
                .ForMember(d => d.Image, opt => opt.Ignore())
                .ForPath(d => d.ShoppingPrice.Amount, opt => opt.MapFrom(s => s.ShoppingPrice))
                .ForPath(d => d.ShoppingPrice.Currency.Code, opt => opt.MapFrom(s => s.Currency))
                .ForPath(d => d.SellingPrice.Amount, opt => opt.MapFrom(s => s.SellingPrice))
                .ForPath(d => d.SellingPrice.Currency.Code, opt => opt.MapFrom(s => s.Currency))
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.Category, opt => opt.Ignore());

        }
    }
}
