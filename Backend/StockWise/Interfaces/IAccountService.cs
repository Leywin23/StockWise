using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.AccountDtos;
using StockWise.Helpers;
using StockWise.Models;
using System.Security.Claims;

namespace StockWise.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResult<string>> ApproveUser(string userId, AppUser currentUser);
        Task<ServiceResult<string>> ChangeUserCompany([FromRoute] long companyNIP, AppUser currentUser);
        Task<ServiceResult<CompanyWithAccountDto>> CreateCompanyWithAccountAsync(CreateCompanyWithAccountDto dto, CancellationToken ct = default);
        Task<ServiceResult<List<AppUser>>> GetAllPending(AppUser user);
        Task<ServiceResult<LoginDto>> Login(LoginDataDto loginDto);
        Task<ServiceResult<string>> LogoutAsync(CancellationToken ct, ClaimsPrincipal ctx, ControllerBase cbase);
        Task<ServiceResult<NewUserDto>> RegisterAsync(RegisterDto userDto, CancellationToken ct = default);
        Task<ServiceResult<string>> RejectUserFromCompany(string userId, AppUser currentUser);
        Task<ServiceResult<string>> RestartPassword(string email, string code, string newPassword);
        Task<ServiceResult<string>> SuspendUserFromCompany(string userId, AppUser currentUser);
        Task<ServiceResult<string>> UnsuspendUserFromCompany(string userId, AppUser currentUser);
        Task<ServiceResult<string>> VerifyEmailAsync(string email, string code);
    }
}