using EmployeePolicyAI.API.Data;
using EmployeePolicyAI.API.DTOs;
using EmployeePolicyAI.API.Models;
using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeePolicyController : ControllerBase
    {
        private readonly IEmployeePolicyService _service;

        public EmployeePolicyController(
            IEmployeePolicyService service)
        {
            _service = service;
        }

        // GET: api/EmployeePolicy
        [HttpGet]
        public async Task<IActionResult> GetPolicies()
        {
            var policies = await _service.GetAllPoliciesAsync();

            return Ok(policies);
        }

        // GET: api/EmployeePolicy/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicy(int id)
        {
            var policy = await _service.GetPolicyByIdAsync(id);

            if (policy == null)
            {
                return NotFound(new
                {
                    message = "Policy not found"
                });
            }

            return Ok(policy);
        }

        // POST: api/EmployeePolicy
        [HttpPost]
        public async Task<IActionResult> CreatePolicy(
            EmployeePolicyDto policyDto)
        {
            var policy = await _service.CreatePolicyAsync(policyDto);

            return CreatedAtAction(
                nameof(GetPolicy),
                new { id = policy.Id },
                policy);
        }

        // PUT: api/EmployeePolicy/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(
            int id,
            EmployeePolicyDto policyDto)
        {
            var policy = await _service.UpdatePolicyAsync(
                id,
                policyDto);

            if (policy == null)
            {
                return NotFound(new
                {
                    message = "Policy not found"
                });
            }

            return Ok(policy);
        }

        // DELETE: api/EmployeePolicy/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var deleted = await _service.DeletePolicyAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Policy not found"
                });
            }

            return Ok(new
            {
                message = "Policy deleted successfully"
            });
        }
    }
}