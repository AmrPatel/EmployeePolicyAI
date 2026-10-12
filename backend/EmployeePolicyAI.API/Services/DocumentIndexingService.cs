using System.Security.Cryptography;
using EmployeePolicyAI.API.Models;

namespace EmployeePolicyAI.API.Services
{
    public class DocumentIndexingService
    {
        private readonly IDocumentService _documentService;
        private readonly DocumentChunkService _chunkService;
        private readonly OllamaEmbeddingService _embeddingService;
        private readonly InMemoryVectorStore _vectorStore;
        private readonly DocumentChunkRepository _repository;

        public DocumentIndexingService(
            IDocumentService documentService,
            DocumentChunkService chunkService,
            OllamaEmbeddingService embeddingService,
            InMemoryVectorStore vectorStore,
            DocumentChunkRepository repository)
        {
            _documentService = documentService;
            _chunkService = chunkService;
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _repository = repository;
        }

        public async Task<int> IndexPdfAsync(
            string documentName,
            string filePath)
        {
            // Step 1: Calculate the PDF's SHA-256 hash.
            await using var stream = File.OpenRead(filePath);

            var hashBytes = await SHA256.HashDataAsync(stream);
            var documentHash = Convert.ToHexString(hashBytes);

            // Step 2: Prevent duplicate indexing.
            var alreadyExists =
                await _repository.DocumentHashExistsAsync(documentHash);

            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    "This PDF has already been indexed.");
            }

            // Step 3: Extract each PDF page separately.
            var pages =
                await _documentService.ExtractPagesFromPdfAsync(filePath);

            if (pages.Count == 0 ||
                pages.All(page => string.IsNullOrWhiteSpace(page.Text)))
            {
                throw new InvalidOperationException(
                    "No text could be extracted from the PDF.");
            }

            // Step 4: Create chunks while preserving page numbers.
            var chunks =
                _chunkService.CreatePageChunks(documentName, pages);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No document chunks could be created.");
            }

            var storedChunks = new List<StoredDocumentChunk>();

            // Step 5: Generate embeddings.
            foreach (var chunk in chunks)
            {
                var embedding =
                    await _embeddingService.GenerateEmbeddingAsync(
                        chunk.Content);

                storedChunks.Add(new StoredDocumentChunk
                {
                    DocumentName = chunk.DocumentName,
                    DocumentHash = documentHash,
                    PageNumber = chunk.PageNumber,
                    ChunkNumber = chunk.ChunkNumber,
                    Content = chunk.Content,
                    Embedding = embedding
                });
            }

            // Step 6: Save chunks in SQL Server.
            await _repository.SaveChunksAsync(storedChunks);

            // Step 7: Add chunks to vector search.
            foreach (var storedChunk in storedChunks)
            {
                _vectorStore.AddChunk(storedChunk);
            }

            return storedChunks.Count;
        }
    }
}
