namespace EmployeePolicyAI.API.Models
{
    public class DocumentChunkEntity
    {
        public int Id { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public string DocumentHash { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public int ChunkNumber { get; set; }

        public string Content { get; set; } = string.Empty;

        public string EmbeddingJson { get; set; } = string.Empty;
    }
}