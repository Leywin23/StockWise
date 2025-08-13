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
    }

}
