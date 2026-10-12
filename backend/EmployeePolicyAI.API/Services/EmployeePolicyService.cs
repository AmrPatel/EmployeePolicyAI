using EmployeePolicyAI.API.Data;
using EmployeePolicyAI.API.DTOs;
using EmployeePolicyAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeePolicyAI.API.Services
{
    public class EmployeePolicyService : IEmployeePolicyService
    {
        private readonly EmployeePolicyDbContext _context;

        public EmployeePolicyService(
            EmployeePolicyDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeePolicy>> GetAllPoliciesAsync()
        {
            return await _context.EmployeePolicies
                .ToListAsync();
        }

        public async Task<EmployeePolicy?> GetPolicyByIdAsync(int id)
        {
            return await _context.EmployeePolicies
                .FindAsync(id);
        }

        public async Task<EmployeePolicy> CreatePolicyAsync(
            EmployeePolicyDto policyDto)
        {
            var policy = new EmployeePolicy
            {
                PolicyName = policyDto.PolicyName,
                Description = policyDto.Description,
                Category = policyDto.Category
            };

            _context.EmployeePolicies.Add(policy);

            await _context.SaveChangesAsync();

            return policy;
        }

        public async Task<EmployeePolicy?> UpdatePolicyAsync(
            int id,
            EmployeePolicyDto policyDto)
        {
            var policy = await _context.EmployeePolicies
                .FindAsync(id);

            if (policy == null)
            {
                return null;
            }

            policy.PolicyName = policyDto.PolicyName;
            policy.Description = policyDto.Description;
            policy.Category = policyDto.Category;

            await _context.SaveChangesAsync();

            return policy;
        }

        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.EmployeePolicies
                .FindAsync(id);

            if (policy == null)
            {
                return false;
            }

            _context.EmployeePolicies.Remove(policy);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}