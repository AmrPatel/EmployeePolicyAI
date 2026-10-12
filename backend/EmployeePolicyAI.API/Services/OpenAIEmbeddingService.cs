using OpenAI;
using OpenAI.Embeddings;

namespace EmployeePolicyAI.API.Services
{
    public class OpenAIEmbeddingService
    {
        private readonly EmbeddingClient _client;

        public OpenAIEmbeddingService(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is not configured.");
            }

            _client = new EmbeddingClient(
                "text-embedding-3-small",
                apiKey);
        }

        public async Task<float[]> GenerateEmbeddingAsync(
            string text)
        {
            var result =
                await _client.GenerateEmbeddingAsync(text);

            return result.Value.ToFloats().ToArray();
        }
    }
}