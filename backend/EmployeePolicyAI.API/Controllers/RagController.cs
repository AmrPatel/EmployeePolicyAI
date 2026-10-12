using EmployeePolicyAI.API.Models;
using EmployeePolicyAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePolicyAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly OllamaEmbeddingService _embeddingService;
        private readonly InMemoryVectorStore _vectorStore;
        private readonly OllamaAnswerService _answerService;

        public RagController(
            OllamaEmbeddingService embeddingService,
            InMemoryVectorStore vectorStore,
            OllamaAnswerService answerService)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _answerService = answerService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] RagRequest request)
        {
            // Step 1: Validate the question.
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new
                {
                    message = "Please enter a question."
                });
            }

            try
            {
                // Step 2: Convert the question into an embedding.
                var queryEmbedding =
                    await _embeddingService.GenerateEmbeddingAsync(
                        request.Question);

                // Step 3: Search for relevant document chunks.
                var topK = request.TopK <= 0
                    ? 3
                    : Math.Min(request.TopK, 5);

                var searchResults = _vectorStore.SearchWithScores(
                    queryEmbedding,
                    topK);

                // Step 4: Check whether documents are indexed.
                if (searchResults.Count == 0)
                {
                    return Ok(new
                    {
                        question = request.Question,
                        answer = "No policy documents have been indexed yet. Please upload and index a policy PDF.",
                        sources = Array.Empty<object>()
                    });
                }

                // Step 5: Keep relevant document chunks.
                const float minimumSimilarity = 0.50f;

                var relevantResults = searchResults
                    .Where(result =>
                        result.Score >= minimumSimilarity)
                    .ToList();

                // Step 6: Do not generate an answer without
                // sufficiently relevant policy information.
                if (relevantResults.Count == 0)
                {
                    return Ok(new
                    {
                        question = request.Question,
                        answer = "I couldn't find sufficiently relevant information in the uploaded policy documents to answer this question reliably.",
                        sources = Array.Empty<object>()
                    });
                }

                // Step 7: Prepare retrieved policy context.
                var context = string.Join(
                    "\n\n",
                    relevantResults.Select(result =>
                        $"Document: {result.Chunk.DocumentName}\n" +
                        $"Chunk number: {result.Chunk.ChunkNumber}\n" +
                        $"Policy text: {result.Chunk.Content}"));

                // Step 8: Validate and limit conversation history.
                // Accept only user and assistant messages.
                var conversationHistory =
                    (request.ConversationHistory ??
                     new List<ChatHistoryMessage>())
                    .Where(message =>
                        message != null &&
                        !string.IsNullOrWhiteSpace(message.Content) &&
                        (string.Equals(
                            message.Role,
                            "user",
                            StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(
                            message.Role,
                            "assistant",
                            StringComparison.OrdinalIgnoreCase)))
                    .TakeLast(8)
                    .Select(message => new ChatHistoryMessage
                    {
                        Role = message.Role.ToLowerInvariant(),
                        Content = message.Content.Length > 3000
                            ? message.Content[..3000]
                            : message.Content
                    })
                    .ToList();

                // Step 9: Generate the answer using retrieved
                // policy context and previous conversation.
                var answer =
                    await _answerService.GenerateAnswerAsync(
                        request.Question,
                        context,
                        conversationHistory);

                // Step 10: Return the answer and source information.
                var sources = relevantResults.Select(result => new
                {
                    documentName = result.Chunk.DocumentName,
                    chunkNumber = result.Chunk.ChunkNumber,
                    PageNumber = result.Chunk.PageNumber,
                    similarityScore = Math.Round(
                        (double)result.Score, 3),
                    content = result.Chunk.Content
                }).ToList();

                return Ok(new
                {
                    question = request.Question,
                    answer,
                    sources
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    message = "Cannot connect to Ollama. Check that Ollama is running and the required models are installed."
                });
            }
        }
    }

    public class RagRequest
    {
        public string Question { get; set; } = string.Empty;

        public int TopK { get; set; } = 3;

        public List<ChatHistoryMessage> ConversationHistory
        {
            get;
            set;
        } = new();
    }
}