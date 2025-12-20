using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Application.Abstractions;
using StockWise.Application.Contracts.AccountDtos;
using StockWise.Application.Contracts.CompanyDtos;
using StockWise.Application.Contracts.CompanyProductDtos;
using StockWise.Application.Contracts.OrderDtos;
using StockWise.Application.Interfaces;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockWise.Infrastructure.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly StockWiseDb _context;
        private readonly IMapper _mapper;
        public CompanyService(StockWiseDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult<CompanyDto>> GetCompanyAsync(AppUser user)
        {
            if(user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<CompanyDto>.Forbidden("You have to be approved by a manager to use this functionality");
            }
            var company = await GetUserCompanyAsync(user);
            if (company == null) return ServiceResult<CompanyDto>.NotFound("User is not assigned to any company");
            var companyDto = _mapper.Map<CompanyDto>(company);
            return ServiceResult<CompanyDto>.Ok(companyDto);
        }

        public async Task<ServiceResult<AdvancedCompanyDto>> GetAdvancedCompanyDataAsync(AppUser user)
        {
            if(user?.CompanyId is null)
                return ServiceResult<AdvancedCompanyDto>.Forbidden("User is not assigned to any company");

            if (user.CompanyMembershipStatus != CompanyMembershipStatus.Approved)
            {
                return ServiceResult<AdvancedCompanyDto>.Forbidden("You have to be approved by a manager to use this functionality");
            }

            var companyId = user.CompanyId.Value;

            var company = await _context.Companies
                .Where(c => c.Id == companyId)
                .AsNoTracking()
                .ProjectTo<AdvancedCompanyDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (company == null)
                return ServiceResult<AdvancedCompanyDto>.NotFound("User is not assigned to any company");

            var ordersAsBuyer = await _context.Orders
                .Where(o => o.BuyerId == companyId)
                .AsNoTracking()
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalPrice.Amount,
                    TotalCurrencyCode = o.TotalPrice.Currency.Code,
                    UserNameWhoMadeOrder = o.UserNameWhoMadeOrder,
                    Counterparty = new CompanyMiniDto
                    {
                        Id = o.SellerId,
                        Name = o.Seller.Name,
                        NIP = o.Seller.NIP,
                    }
                })
                .ToListAsync();

            var ordersAsSeller = await _context.Orders
                .Where(o => o.SellerId == companyId)
                .AsNoTracking()
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    TotalAmount = o.TotalPrice.Amount,
                    TotalCurrencyCode = o.TotalPrice.Currency.Code,
                    UserNameWhoMadeOrder = o.UserNameWhoMadeOrder,
                    Counterparty = new CompanyMiniDto
                    {
                        Id = o.BuyerId,
                        Name = o.Buyer.Name,
                        NIP = o.Buyer.NIP,
                    }

                })
                .ToListAsync();

            var users = await _context.Users
                .Where(u => u.CompanyId == companyId)
                .AsNoTracking()
                .Select(u => new CompanyUserDto
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    CompanyMembershipStatus = u.CompanyMembershipStatus,
                })
                .ToListAsync();

            company.OrdersAsBuyer = ordersAsBuyer;
            company.OrdersAsSeller = ordersAsSeller;
            company.Users = users;

            return ServiceResult<AdvancedCompanyDto>.Ok(company);
        }


        public async Task<ServiceResult<CompanyDto>> CreateCompanyAsync(CreateCompanyDto companyDto)
        {
            var exists = await _context.Companies.AnyAsync(c =>
                c.NIP == companyDto.NIP ||
                c.Email == companyDto.Email ||
                c.Phone == companyDto.Phone ||
                c.Name == companyDto.Name);

            if (exists) return ServiceResult<CompanyDto>.Conflict("Company with this data already exist");

            var newCompany = new Company
            {
                Name = companyDto.Name,
                NIP = companyDto.NIP,
                Address = companyDto.Address,
                Email = companyDto.Email,
                Phone = companyDto.Phone,
                Verified = true
            };

            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();

            var newCompanyDto = _mapper.Map<CompanyDto>(newCompany);

            return ServiceResult<CompanyDto>.Ok(newCompanyDto);
        }

        public async Task<ServiceResult<CompanyDto>> DeleteCompanyAsync(AppUser user)
        {
            var company = await _context.Companies
                .Include(c => c.CompanyProducts)
                .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

            if (company == null)
                return ServiceResult<CompanyDto>.NotFound("User is not assigned to any company");

            _context.CompanyProducts.RemoveRange(company.CompanyProducts);

            var users = await _context.Users
               .Where(u => u.CompanyId == company.Id)
               .ToListAsync();

            foreach (var u in users)
            {
                u.CompanyId = null;
                u.CompanyMembershipStatus = CompanyMembershipStatus.Suspended;
            }
            await _context.SaveChangesAsync();

            _context.Users.UpdateRange(users);

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<CompanyDto>(company);
            return ServiceResult<CompanyDto>.Ok(dto);
        }


        public async Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(UpdateCompanyDto companyDto, AppUser user)
        {
            var company = await GetUserCompanyAsync(user);
            if (company == null) return ServiceResult<CompanyDto>.NotFound("User is not assigned to any company");

            var companyNameCheck = await _context.Companies.FirstOrDefaultAsync(c=>c.Name == companyDto.Name);
            if (companyNameCheck != null)
                return ServiceResult<CompanyDto>.Conflict("Company with this name already exist");

            company.Name = companyDto.Name;
            company.Email = companyDto.Email;
            company.Phone = companyDto.Phone;
            company.Address = companyDto.Address;

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            var updatedCompanyDto = _mapper.Map<CompanyDto>(company);

            return ServiceResult<CompanyDto>.Ok(updatedCompanyDto);
        }

        private async Task<Company?> GetUserCompanyAsync(AppUser user)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);
        }

        public async Task<ServiceResult<PageResult<Company>>> GetAllCompanyAsync(CompanyQueryParams q)
        {
            q.Page = q.Page <= 0 ? 1 : q.Page;
            q.PageSize = q.PageSize switch
            {
                <= 10 => 10,
                > 100 => 100,
                _ => q.PageSize
            };

            var query = _context.Companies.AsNoTracking();

            if (q.WithOrdersAsBuyer) query = query.Include(c => c.OrdersAsBuyer);
            if (q.WithOrdersAsSeller) query = query.Include(c => c.OrdersAsSeller);
            if (q.WithCompanyProducts) query = query.Include(c => c.CompanyProducts);
            if (q.WithCompanyUsers) query = query.Include(c => c.Users);

            query = q.SortedBy switch
            {
                "Name" => q.SortDir == SortDir.Desc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),

                "CreatedAt" => q.SortDir == SortDir.Desc
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),

                _ => q.SortDir == SortDir.Desc
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id)
            };

            query = query.AsSplitQuery();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .ToListAsync();

            var result = new PageResult<Company>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = totalCount,
                SortBy = q.SortedBy ?? nameof(Company.Id),
                SortDir = q.SortDir,
                Items = items
            };

            return ServiceResult<PageResult<Company>>.Ok(result);
        }

    }

}
