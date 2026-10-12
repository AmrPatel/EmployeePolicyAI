namespace EmployeePolicyAI.API.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public int ChunkNumber { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
