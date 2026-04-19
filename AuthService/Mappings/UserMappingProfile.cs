using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;

namespace AuthService.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.ActiveCompany, opt => opt.Ignore()) // Manuel set ediliyor
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());
        CreateMap<UserDto, User>();
        CreateMap<RegisterDto, User>()
                    .ForMember(dest => dest.Password, opt => opt.Ignore()) // Manuel hash'leniyor
                    .ForMember(dest => dest.Companies, opt => opt.Ignore()); // Manuel ekleniyor
        CreateMap<RegisterDto, UserDto>();
    }
}
