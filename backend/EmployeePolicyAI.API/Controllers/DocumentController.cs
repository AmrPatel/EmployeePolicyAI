using EmployeePolicyAI.API.DTOs;
using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly DocumentChunkService _chunkService;

        public DocumentController(
            IDocumentService documentService,
            DocumentChunkService chunkService)
        {
            _documentService = documentService;
            _chunkService = chunkService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please upload a file."
                });
            }

            if (!Path.GetExtension(file.FileName)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only PDF files are supported."
                });
            }

            var documentsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Documents");

            Directory.CreateDirectory(documentsPath);

            var fileName = Guid.NewGuid() +
                           Path.GetExtension(file.FileName);

            var filePath = Path.Combine(
                documentsPath,
                fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract text
            var extractedText =
                await _documentService
                    .ExtractTextFromPdfAsync(filePath);

            // Create chunks
            var chunks = _chunkService.CreateChunks(
                file.FileName,
                extractedText);

            return Ok(new
            {
                fileName = file.FileName,
                characterCount = extractedText.Length,
                chunkCount = chunks.Count,
                chunks = chunks
            });
        }
    }
}