using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;

namespace AuthService.Mappings;

public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<CompanyDto, Company>()
        .ForAllMembers(opts =>
            opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UserCompany, CompanyDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.CRole, opt => opt.MapFrom(src => src.Role));
        CreateMap<UserCompany, Company>();
        CreateMap<Company, UserCompany>();
    }
}

