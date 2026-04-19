using AuthService.DTOs;
using AuthService.Models;
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