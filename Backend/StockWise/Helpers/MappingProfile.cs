using AutoMapper;
using StockWise.Models;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.CompanyDtos;

namespace StockWise.Helpers
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<CompanyProduct, CompanyProductDto>();

            CreateMap<CompanyProduct, CompanyProductDto>()
                .ForMember(dest => dest.CompanyProductName, opt => opt.MapFrom(src=>src.CompanyProductName))
                .ForMember(dest => dest.EAN, opt => opt.MapFrom(src => src.EAN))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock))
                .ForMember(dest => dest.IsAvailableForOrder, opt => opt.MapFrom(src => src.IsAvailableForOrder));

            CreateMap<Company, CompanyDto>();

            CreateMap(typeof(PageResult<>), typeof(PageResult<>));

            CreateMap<CompanyProductDto, CompanyProduct>();
        }
    }
}
