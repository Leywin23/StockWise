using Microsoft.AspNetCore.Mvc;
using StockWise.Dtos.AccountDtos;
using StockWise.Dtos.CompanyProductDtos;
using StockWise.Dtos.OrderDtos;
using StockWise.Models;
using StockWise.Response;
using System.Security.Claims;

namespace StockWise.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResult<string>> ApproveUserAsync(string userId);
        Task<ServiceResult<string>> ChangeUserCompanyAsync([FromRoute] long companyNIP);
        Task<ServiceResult<CompanyWithAccountDto>> CreateCompanyWithAccountAsync(CreateCompanyWithAccountDto dto, CancellationToken ct = default);
        Task<ServiceResult<List<CompanyUserDto>>> GetAllPendingAsync();
        Task<ServiceResult<LoginDto>> LoginAsync(LoginDataDto loginDto);
        Task<ServiceResult<string>> LogoutAsync(CancellationToken ct, ControllerBase cbase);
        Task<ServiceResult<NewUserDto>> RegisterAsync(RegisterDto userDto, CancellationToken ct = default);
        Task<ServiceResult<string>> RejectUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> SendRequestToRestartPasswordAsync(string email);
        Task<ServiceResult<string>> RestartPasswordAsync(string email, string code, string newPassword);
        Task<ServiceResult<string>> SuspendUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> UnsuspendUserFromCompanyAsync(string userId);
        Task<ServiceResult<string>> VerifyEmailAsync(string email, string code, CancellationToken ct = default);

    }
}