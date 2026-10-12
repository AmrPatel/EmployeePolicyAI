using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace EmployeePolicyAI.API.Services
{
    public class OllamaEmbeddingService
    {
        private readonly HttpClient _httpClient;

        public OllamaEmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Text cannot be empty.",
                    nameof(text));
            }

            var request = new OllamaEmbeddingRequest
            {
                Model = "nomic-embed-text",
                Input = text
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/embed",
                request);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();

            if (result?.Embeddings == null ||
                result.Embeddings.Length == 0)
            {
                throw new InvalidOperationException(
                    "Ollama returned no embedding.");
            }

            return result.Embeddings[0];
        }

        private class OllamaEmbeddingRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;
        }

        private class OllamaEmbeddingResponse
        {
            [JsonPropertyName("embeddings")]
            public float[][] Embeddings { get; set; }
                = Array.Empty<float[]>();
        }
    }
}