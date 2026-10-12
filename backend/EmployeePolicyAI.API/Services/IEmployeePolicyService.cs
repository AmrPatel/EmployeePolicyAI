using EmployeePolicyAI.API.DTOs;
using EmployeePolicyAI.API.Models;

namespace EmployeePolicyAI.API.Services
{
    public interface IEmployeePolicyService
    {
        Task<List<EmployeePolicy>> GetAllPoliciesAsync();

        Task<EmployeePolicy?> GetPolicyByIdAsync(int id);

        Task<EmployeePolicy> CreatePolicyAsync(
            EmployeePolicyDto policyDto);

        Task<EmployeePolicy?> UpdatePolicyAsync(
            int id,
            EmployeePolicyDto policyDto);

        Task<bool> DeletePolicyAsync(int id);
    }
}