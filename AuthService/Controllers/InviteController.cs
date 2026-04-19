using AuthService.DTOs;
//using AuthService.Filters;
using AuthService.Models;
using AuthService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.Models;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class InviteController : ControllerBase
{
    private readonly IInviteRepository _inviteRepository;
    private readonly IMapper _mapper;

    public InviteController(IInviteRepository inviteRepository, IMapper mapper)
    {
        _inviteRepository = inviteRepository;
        _mapper = mapper;
    }


    [HttpPost("create")]
    public async Task<IActionResult> CreateInvite(InviteDto dto)
    {
        if (string.IsNullOrEmpty(dto.CompanyId)) return BadRequest(ReturnObject<Invite>.Fail("FirmaId Bilgisi Hatalı"));

        var invite = _mapper.Map<Invite>(dto);
        invite.Token = Guid.NewGuid().ToString();
        var result = await _inviteRepository.AddAsync(invite);
        return Ok(ReturnObject<InviteDto>.Success(_mapper.Map<InviteDto>(result)));
    }

    [HttpGet("{companyId}")]
    public async Task<IActionResult> GetByCompany(string companyId)
    {
        var invites = (await _inviteRepository.FindAsync(x => x.CompanyId == companyId)).ToList();
        return Ok(ReturnObject<List<InviteDto>>.Success(_mapper.Map<List<InviteDto>>(invites)));
    }
}
