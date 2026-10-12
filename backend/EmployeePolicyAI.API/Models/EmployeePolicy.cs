namespace EmployeePolicyAI.API.Models
{
    public class EmployeePolicy
    {
        public int Id { get; set; }

        public string PolicyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}
