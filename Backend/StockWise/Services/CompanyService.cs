using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;
using StockWise.Dtos.CompanyDtos;
using StockWise.Interfaces;
using StockWise.Models;

namespace StockWise.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly StockWiseDb _context;

        public CompanyService(StockWiseDb context)
        {
            _context = context;
        }

        public async Task<Company?> CreateCompanyAsync(CreateCompanyDto companyDto)
        {
            var exists = await _context.Companies.AnyAsync(c =>
                c.NIP == companyDto.NIP ||
                c.Email == companyDto.Email ||
                c.Phone == companyDto.Phone ||
                c.Name == companyDto.Name);

            if (exists) return null;

            var newCompany = new Company
            {
                Name = companyDto.Name,
                NIP = companyDto.NIP,
                Address = companyDto.Address,
                Email = companyDto.Email,
                Phone = companyDto.Phone,
            };

            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();

            return newCompany;
        }

        public async Task<Company?> DeleteCompanyAsync(AppUser user)
        {
            var company = await GetUserCompanyAsync(user);
            if (company == null) return null;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company?> GetCompanyAsync(AppUser user)
        {
            return await GetUserCompanyAsync(user);
        }

        public async Task<Company?> UpdateCompanyAsync(UpdateCompanyDto companyDto, AppUser user)
        {
            var company = await GetUserCompanyAsync(user);
            if (company == null) return null;

            company.Name = companyDto.Name;
            company.Email = companyDto.Email;
            company.Phone = companyDto.Phone;
            company.Address = companyDto.Address;

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return company;
        }

        private async Task<Company?> GetUserCompanyAsync(AppUser user)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.Company.Id);
        }

        public async Task<PageResult<Company>> GetAllCompanyAsync(CompanyQueryParams q)
        {
            q.Page = q.Page <= 0 ? 1 : q.Page;
            q.PageSize = q.PageSize switch
            {
                <= 0 => 10,
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

            return new PageResult<Company>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = totalCount,
                SortBy = q.SortedBy ?? nameof(Company.Id),
                SortDir = q.SortDir,
                Items = items
            };
        }

    }

}
