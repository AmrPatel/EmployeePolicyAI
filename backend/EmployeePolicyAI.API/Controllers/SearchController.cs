using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly OllamaEmbeddingService _embeddingService;
        private readonly InMemoryVectorStore _vectorStore;

        public SearchController(
            OllamaEmbeddingService embeddingService,
            InMemoryVectorStore vectorStore)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            [FromBody] SearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new
                {
                    message = "Question is required."
                });
            }

            var queryEmbedding = await _embeddingService
                .GenerateEmbeddingAsync(request.Question);

            var results = _vectorStore.Search(
                queryEmbedding,
                request.TopK);

            return Ok(new
            {
                question = request.Question,
                resultCount = results.Count,
                results = results.Select(chunk => new
                {
                    chunk.DocumentName,
                    chunk.ChunkNumber,
                    chunk.Content
                })
            });
        }
    }

    public class SearchRequest
    {
        public string Question { get; set; } = string.Empty;

        public int TopK { get; set; } = 3;
    }
}