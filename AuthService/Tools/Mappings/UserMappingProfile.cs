using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;
using Shared.Extensions;

namespace AuthService.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.ActiveCompany, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());
        CreateMap<UserDto, User>();
        CreateMap<UserInsertDto, User>()
                    .ForMember(dest => dest.Password, opt => opt.Ignore()) 
                    .ForMember(dest => dest.Companies, opt => opt.Ignore()); 
        CreateMap<UserInsertDto, UserDto>();

        CreateMap<User, UserSummaryDto>()
    .ForMember(d => d.MaskedEmail, o => o.MapFrom(s => s.Email.MaskEmail()))
    .ForMember(d => d.MaskedPhone, o => o.MapFrom(s => s.PhoneNumber.MaskPhone()));
    }
}
