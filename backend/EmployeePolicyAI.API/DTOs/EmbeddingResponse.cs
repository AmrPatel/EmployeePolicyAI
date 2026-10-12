namespace EmployeePolicyAI.API.DTOs
{
    public class EmbeddingResponse
    {
        public string Text { get; set; } = string.Empty;

        public int Dimensions { get; set; }

        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}