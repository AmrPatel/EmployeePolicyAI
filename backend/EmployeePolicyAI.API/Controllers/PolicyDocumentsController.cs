using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyDocumentsController : ControllerBase
    {
        private readonly DocumentIndexingService _indexingService;

        public PolicyDocumentsController(
            DocumentIndexingService indexingService)
        {
            _indexingService = indexingService;
        }

        [HttpPost("upload-and-index")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadAndIndex(
            [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a PDF file."
                });
            }

            if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only PDF files are supported."
                });
            }

            // Limit uploads to 20 MB.
            if (file.Length > 20 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    message = "The PDF must be 20 MB or smaller."
                });
            }

            var documentsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Documents");

            Directory.CreateDirectory(documentsFolder);

            // Generate a safe server-side filename.
            var savedFileName =
                $"{Guid.NewGuid()}.pdf";

            var filePath = Path.Combine(
                documentsFolder,
                savedFileName);

            try
            {
                // 1. Save the PDF.
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                // 2. Extract text, create chunks, generate embeddings,
                //    and add them to the vector store.
                var chunkCount = await _indexingService.IndexPdfAsync(
                    file.FileName,
                    filePath);

                if (chunkCount == 0)
                {
                    return BadRequest(new
                    {
                        message = "No readable text was found in the PDF."
                    });
                }

                // 3. Return the result.
                return Ok(new
                {
                    message = "PDF uploaded and indexed successfully.",
                    documentName = file.FileName,
                    chunkCount
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    message = "Cannot connect to Ollama. Check that Ollama is running and the embedding model is installed."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while processing the PDF."
                });
            }
        }
    }
}