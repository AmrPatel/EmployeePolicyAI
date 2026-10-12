using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndexingController : ControllerBase
    {
        private readonly DocumentIndexingService _indexingService;

        public IndexingController(
            DocumentIndexingService indexingService)
        {
            _indexingService = indexingService;
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexPdf(
            [FromBody] IndexDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath) ||
                string.IsNullOrWhiteSpace(request.DocumentName))
            {
                return BadRequest(new
                {
                    message = "Document name and file path are required."
                });
            }

            try
            {
                var count = await _indexingService.IndexPdfAsync(
                    request.DocumentName,
                    request.FilePath);

                return Ok(new
                {
                    message = "Document indexed successfully.",
                    documentName = request.DocumentName,
                    chunkCount = count
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    message = "Cannot connect to Ollama."
                });
            }
        }
    }

    public class IndexDocumentRequest
    {
        public string DocumentName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;
    }
}