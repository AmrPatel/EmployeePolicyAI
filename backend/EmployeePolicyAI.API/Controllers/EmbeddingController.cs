using EmployeePolicyAI.API.DTOs;
using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmbeddingController : ControllerBase
    {
        private readonly OllamaEmbeddingService _embeddingService;

        public EmbeddingController(
            OllamaEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateEmbedding(
            [FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new
                {
                    message = "Text is required."
                });
            }

            try
            {
                var embedding = await _embeddingService
                    .GenerateEmbeddingAsync(text);

                var response = new EmbeddingResponse
                {
                    Text = text,
                    Dimensions = embedding.Length,
                    Embedding = embedding
                };

                return Ok(response);
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    message =
                        "Cannot connect to Ollama. Make sure Ollama is running."
                });
            }
        }
    }
}