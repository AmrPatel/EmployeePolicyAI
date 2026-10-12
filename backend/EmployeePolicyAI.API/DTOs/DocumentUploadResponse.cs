namespace EmployeePolicyAI.API.DTOs
{
    public class DocumentUploadResponse
    {
        public string FileName { get; set; } = string.Empty;

        public int CharacterCount { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}