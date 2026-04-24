namespace CustomerService.Data.Services.Interfaces;

public interface ICompanyService
{
    Task InitializeForNewUserAsync(string userId, string? name, string? surname, string? inviteCode);
}
