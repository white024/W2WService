using AuthService.Models;
using AuthService.Models.DTOs;
using AutoMapper;

namespace AuthService.Mappings;

public class InviteMappingProfile : Profile
{
    public InviteMappingProfile()
    {
        CreateMap<Invite, InviteDto>();
        CreateMap<InviteDto, Invite>();
    }
}