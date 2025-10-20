using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StockWise.Data;
using StockWise.Dtos.AccountDtos;
using StockWise.Interfaces;
using StockWise.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using StockWise.Response;

namespace StockWise.Services
{
    public class AccountService : IAccountService
    {
        private readonly ITokenService _tokenService;
        private readonly StockWiseDb _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSenderServicer _emailSenderServicer;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUserService;
        public AccountService(ITokenService tokenService,
            StockWiseDb context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, 
            IEmailSenderServicer emailSenderServicer,
            IMemoryCache cache,
            ICurrentUserService currentUserService)
        {
            _tokenService = tokenService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSenderServicer = emailSenderServicer;
            _cache = cache;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResult<NewUserDto>> RegisterAsync(RegisterDto userDto, CancellationToken ct = default)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.NIP == userDto.CompanyNIP, ct);

            if (company is null)
                return ServiceResult<NewUserDto>.NotFound($"There isn't any company with NIP: {userDto.CompanyNIP}");

            var existingByEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingByEmail is not null)
                return ServiceResult<NewUserDto>.Conflict("Registration failed. Email already exists.");

            var existingByName = await _userManager.FindByNameAsync(userDto.UserName);
            if (existingByName is not null)
                return ServiceResult<NewUserDto>.Conflict("Registration failed. Username already exists.");

            var newUser = new AppUser
            {
                Email = userDto.Email,
                UserName = userDto.UserName,
                CompanyId = company.Id,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(newUser, userDto.Password);
            if (!createResult.Succeeded)
                return ServiceResult<NewUserDto>.BadRequest("Creating user failed.", createResult.Errors);

            var roleResult = await _userManager.AddToRoleAsync(newUser, "Worker");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);
                return ServiceResult<NewUserDto>.BadRequest("Role assignment failed.", roleResult.Errors);
            }

            try
            {
                var emailToken = GenerateVerifyCode();
                _cache.Set(newUser.Email, emailToken, TimeSpan.FromMinutes(10));
                await _emailSenderServicer.SendEmailAsync(
                    newUser.Email!,
                    "Confirm your email",
                    $"Your confirmation token: {Uri.EscapeDataString(emailToken)}"
                );

                return ServiceResult<NewUserDto>.Ok(new NewUserDto
                {
                    UserName = newUser.UserName!,
                    Email = newUser.Email!,
                    CompanyName = company.Name
                });
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(newUser);
                ;
                return ServiceResult<NewUserDto>.ServerError("User creation failed.");
            }
        }

        public async Task<ServiceResult<LoginDto>> LoginAsync(LoginDataDto loginDto)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    return ServiceResult<LoginDto>.BadRequest("Email or Password is wrong");
                }
                if (user.EmailConfirmed == false) return ServiceResult<LoginDto>.Unauthorized("Email is not confirmed");
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    return ServiceResult<LoginDto>.Unauthorized("Email not found and/or password incorrect");
                }
                return ServiceResult<LoginDto>.Ok(new LoginDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = await _tokenService.CreateToken(user)
                });
            }
            catch (Exception e)
            {
                return ServiceResult<LoginDto>.ServerError(e.Message);
            }
        }

        public async Task<ServiceResult<string>> LogoutAsync(
        CancellationToken ct,
        ControllerBase cbase)
        {
            var jti = cbase.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrEmpty(jti))
                return ServiceResult<string>.Unauthorized("Invalid token: missing JTI claim.");

            var expUnix = cbase.User.FindFirstValue(JwtRegisteredClaimNames.Exp);
            if (!long.TryParse(expUnix, out var expUnixLong))
                return ServiceResult<string>.Unauthorized("Invalid token: missing or invalid expiration claim.");

            var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnixLong).UtcDateTime;

            var alreadyRevoked = await _context.RevokedTokens
                .AsNoTracking()
                .AnyAsync(t => t.Jti == jti, ct);

            if (alreadyRevoked)
            {
                return ServiceResult<string>.BadRequest("This token has already been revoked.");
            }

            var userId = cbase.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<string>.Unauthorized("Invalid token: missing user identifier.");

            var revoked = new RevokedToken
            {
                Jti = jti,
                ExpiresAtUtc = expUtc,
                Reason = "logout",
                UserId = userId
            };

            _context.RevokedTokens.Add(revoked);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<string>.Ok("Logged out successfully. Token has been revoked.");
        }
        public async Task<ServiceResult<string>> SendRequestToRestartPasswordAsync(string email) 
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResult<string>.BadRequest("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var code = GenerateVerifyCode();

                _cache.Set(GetResetKey(email), code, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                await _emailSenderServicer.SendEmailAsync(email, "Reset password code", code);
            }

            return ServiceResult<string>.Ok("If the email exists, a reset code was sent.");
        }
        public async Task<ServiceResult<string>> VerifyEmailAsync(string email, string code, CancellationToken ct = default)
        {
            var user = await _userManager.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return ServiceResult<string>.NotFound("Email isn't assigned to any account.");

            if (user.EmailConfirmed)
                return ServiceResult<string>.BadRequest("Email already verified.");

            if (!_cache.TryGetValue(email, out string storedCode))
                return ServiceResult<string>.BadRequest("Verification code not found or expired.");

            if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                return ServiceResult<string>.BadRequest("Verification code not found or expired.");

            user.EmailConfirmed = true;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
                return ServiceResult<string>.ServerError("Failed to update user email confirmation.");

            _cache.Remove(email);

            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            if (isManager && user.Company!= null)
            {
                user.Company.Verified = true;
                _context.Companies.Update(user.Company);
                await _context.SaveChangesAsync(ct);
            }

            return ServiceResult<string>.Ok("Email verified successfully.");
        }
        public async Task<ServiceResult<string>> RestartPasswordAsync(string email, string code, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return ServiceResult<string>.NotFound("Email isn't assigned to any account");

            var cacheKey = GetResetKey(email);
            if (!_cache.TryGetValue(cacheKey, out string storedCode))
                return ServiceResult<string>.BadRequest("Verification code not found or expired.");

            if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                return ServiceResult<string>.BadRequest("Invalid verification code.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            try
            {
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.BadRequest(ex.Message);
            }

            _cache.Remove(cacheKey);
            return ServiceResult<string>.Ok("Password has been reset successfully.");
        }
        private static string GetResetKey(string email) => $"pwdreset:{email}";
        public static string GenerateVerifyCode(int length = 6)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUWXYZ23456789";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var span = new char[length];
            for (int i = 0; i < length; i++)
            {
                span[i] = alphabet[bytes[i] % alphabet.Length];
            }
            return new string(span);
        }
        public async Task<ServiceResult<string>> ApproveUserAsync(string userId)
        {
            var currentUser = await _currentUserService.EnsureAsync();
            if (currentUser.CompanyId is null)
                return ServiceResult<string>.BadRequest("You are not assigned to a company.");

            var userToApprove = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToApprove is null)
                return ServiceResult<string>.NotFound("User not found.");

            if (userToApprove.CompanyId != currentUser.CompanyId)
                return ServiceResult<string>.BadRequest("User does not belong to your company.");

            if (userToApprove.CompanyMembershipStatus != CompanyMembershipStatus.Pending)
                return ServiceResult<string>.BadRequest("User is not in Pending status.");

            userToApprove.CompanyMembershipStatus = CompanyMembershipStatus.Approved;

            try
            {
                var result = await _userManager.UpdateAsync(userToApprove);
            }
            catch (Exception ex)
            {
                ServiceResult<string>.ServerError(ex.Message);
            }

            return ServiceResult<string>.Ok($"User {userToApprove.UserName} approved");
        }

        public async Task<ServiceResult<List<CompanyUserDto>>> GetAllPendingAsync()
        {
            var user = await _currentUserService.EnsureAsync();
            if (user.CompanyId is null) return ServiceResult<List<CompanyUserDto>>.BadRequest("You are not assigned to a company.");

            var pendings = await _context.Users
                .Where(u => u.CompanyId == user.CompanyId && u.CompanyMembershipStatus == CompanyMembershipStatus.Pending)
                .Select(u=> new CompanyUserDto
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    CompanyMembershipStatus = u.CompanyMembershipStatus
                })
                .ToListAsync();

            if (!pendings.Any()) return ServiceResult<List<CompanyUserDto>>.NotFound("There aren't any pending users.");

            return ServiceResult<List<CompanyUserDto>>.Ok(pendings);
        }

        public async Task<ServiceResult<string>> SuspendUserFromCompanyAsync(string userId)
        {
            var currentUser = await _currentUserService.EnsureAsync();
            var user = await _userManager.FindByIdAsync(userId);
            if (user.Id == currentUser.Id)
                return ServiceResult<string>.BadRequest("You cannot suspend yourself.");

            if (user is null) return ServiceResult<string>.NotFound("User not found");
            if (user.CompanyId != currentUser.CompanyId) return ServiceResult<string>.BadRequest("User isn't aligned to your company");
            if (user.CompanyMembershipStatus == CompanyMembershipStatus.Suspended)
                return ServiceResult<string>.BadRequest("User is already suspended.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Suspended;
            try
            {
                var result = await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.BadRequest(ex.Message);
            }

            return ServiceResult<string>.Ok($"User {user.UserName} has been suspended.");
        }
        public async Task<ServiceResult<string>> UnsuspendUserFromCompanyAsync(string userId)
        {
            var currentUser = await _currentUserService.EnsureAsync();
            if (currentUser.CompanyId is null) return ServiceResult<string>.BadRequest("You are not assigned to a company.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return ServiceResult<string>.NotFound("User not found.");
            if (user.Id == currentUser.Id) return ServiceResult<string>.BadRequest("You cannot unsuspend yourself.");
            if (user.CompanyId != currentUser.CompanyId) return ServiceResult<string>.Forbidden("User does not belong to your company.");
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Suspended)
                return ServiceResult<string>.BadRequest("User is not suspended.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Approved;
            try
            {
                var result = await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.BadRequest(ex.Message);
            }

            return ServiceResult<string>.Ok($"User {user.UserName} has been unsuspended.");
        }

        public async Task<ServiceResult<string>> RejectUserFromCompanyAsync(string userId)
        {
            var currentUser = await _currentUserService.EnsureAsync();
            if (currentUser.CompanyId is null) return ServiceResult<string>.BadRequest("You are not assigned to a company.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return ServiceResult<string>.NotFound("User not found.");
            if (user.Id == currentUser.Id) return ServiceResult<string>.BadRequest("You cannot reject yourself.");
            if (user.CompanyId != currentUser.CompanyId) return ServiceResult<string>.Forbidden("User does not belong to your company.");

            if (user.CompanyMembershipStatus == CompanyMembershipStatus.Rejected)
                return ServiceResult<string>.BadRequest("User is already rejected.");
            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Pending)
                return ServiceResult<string>.BadRequest("Only users in Pending status can be rejected.");

            user.CompanyMembershipStatus = CompanyMembershipStatus.Rejected;
            try
            {
                var result = await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.BadRequest(ex.Message);
            }
            return ServiceResult<string>.Ok($"User {user.UserName} has been rejected.");
        }
        public async Task<ServiceResult<string>> ChangeUserCompanyAsync([FromRoute] long companyNIP)
        {
            var currentUser = await _currentUserService.EnsureAsync();
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == companyNIP);
            if (company is null) return ServiceResult<string>.NotFound("Company with this NIP not found.");

            if (currentUser.CompanyId == company.Id)
                return ServiceResult<string>.BadRequest("You are already assigned to this company.");

            currentUser.CompanyId = company.Id;
            currentUser.Company = company;
            currentUser.CompanyMembershipStatus = CompanyMembershipStatus.Pending;

            try
            {
                var result = await _userManager.UpdateAsync(currentUser);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.BadRequest(ex.Message);
            }

            return ServiceResult<string>.Ok($"User {currentUser.UserName} has changed company to {company.Name} (pending approval).");
        }
        public async Task<ServiceResult<CompanyWithAccountDto>> CreateCompanyWithAccountAsync(CreateCompanyWithAccountDto dto, CancellationToken ct = default)
        {
            var existByNIP = await _context.Companies.FirstOrDefaultAsync(c => c.NIP == dto.NIP);
            if (existByNIP != null)
            {
                return ServiceResult<CompanyWithAccountDto>.Conflict("Comapny with this NIP already exist");
            }
            var existByCompanyName = await _context.Companies.FirstOrDefaultAsync(c => c.Name == dto.CompanyName);
            if (existByCompanyName != null)
            {
                return ServiceResult<CompanyWithAccountDto>.Conflict("Comapny with this name already exist");
            }

            var existByUserName = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existByUserName != null)
            {
                return ServiceResult<CompanyWithAccountDto>.Conflict("User with this name already exist");
            }
            var existByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existByEmail != null)
            {
                return ServiceResult<CompanyWithAccountDto>.Conflict("User with this email already exist");
            }

            var newCompany = new Company
            {
                Name = dto.CompanyName,
                NIP = dto.NIP,
                Email = dto.CompanyEmail,
                Address = dto.Address,
                Phone = dto.Phone,
                CreatedAt = DateTime.Now,
                Verified = false
            };
            var companyResult = await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync(ct);

            var newUser = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Company = newCompany,
                CompanyMembershipStatus = CompanyMembershipStatus.Approved
            };
            try
            {
                await _userManager.CreateAsync(newUser, dto.Password);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyWithAccountDto>.ServerError(ex.Message);
            }
            await _context.SaveChangesAsync(ct);

            try
            {
                await _userManager.AddToRoleAsync(newUser, "Manager");
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyWithAccountDto>.ServerError(ex.Message);
            }

            try
            {
                var emailToken = GenerateVerifyCode();
                _cache.Set(newUser.Email, emailToken, TimeSpan.FromMinutes(10));
                await _emailSenderServicer.SendEmailAsync(dto.Email, "Confirm your email",
                    $"Your confirmation token: {Uri.EscapeDataString(emailToken)}", ct);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyWithAccountDto>.ServerError(ex.Message);
            }
            var result = new CompanyWithAccountDto
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                CompanyName = newCompany.Name,
                NIP = newCompany.NIP,
                CompanyEmail = newCompany.Email,
                Address = newCompany.Address,
                Phone = newCompany.Phone,
            };

            return ServiceResult<CompanyWithAccountDto>.Ok(result);
        }
    }
}
