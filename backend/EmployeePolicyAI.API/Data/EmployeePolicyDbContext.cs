using EmployeePolicyAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeePolicyAI.API.Data
{
    public class EmployeePolicyDbContext : DbContext
    {
        public EmployeePolicyDbContext(
            DbContextOptions<EmployeePolicyDbContext> options)
            : base(options)
        {
        }

        public DbSet<EmployeePolicy> EmployeePolicies { get; set; } //Table Name.
        public DbSet<DocumentChunkEntity> DocumentChunks { get; set; }
    }
}
