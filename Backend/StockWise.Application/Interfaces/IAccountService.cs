
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.AccountDtos;
using System.Security.Claims;

namespace StockWise.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResult<string>> ApproveUserAsync(string userId);
        Task<ServiceResult<string>> ChangeUserCompanyAsync([FromRoute] string companyNIP);
        Task<ServiceResult<CompanyWithAccountDto>> CreateCompanyWithAccountAsync(CreateCompanyWithAccountDto dto, CancellationToken ct = default);
        Task<ServiceResult<List<CompanyUserDto>>> GetAllPendingAsync();
        Task<ServiceResult<LoginDto>> LoginAsync(LoginDataDto loginDto);
        Task<ServiceResult<string>> LogoutAsync(CancellationToken ct = default);
        Task<ServiceResult<NewUserDto>> RegisterAsync(RegisterDto userDto, CancellationToken ct = default);
        Task<ServiceResult<string>> RejectUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> SendRequestToRestartPasswordAsync(string email);
        Task<ServiceResult<string>> RestartPasswordAsync(string email, string code, string newPassword);
        Task<ServiceResult<string>> SuspendUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> UnsuspendUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> VerifyEmailAsync(string email, string code, CancellationToken ct = default);

    }
}