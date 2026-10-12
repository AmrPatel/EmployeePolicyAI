using EmployeePolicyAI.API.Data;
using EmployeePolicyAI.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployeePolicyAI.API.Services
{
    public class DocumentChunkRepository
    {
        private readonly EmployeePolicyDbContext _dbContext;

        public DocumentChunkRepository(
            EmployeePolicyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChunksAsync(
            List<StoredDocumentChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                var entity = new DocumentChunkEntity
                {
                    DocumentName = chunk.DocumentName,
                    PageNumber = chunk.PageNumber,
                    DocumentHash = chunk.DocumentHash,
                    ChunkNumber = chunk.ChunkNumber,
                    Content = chunk.Content,
                    EmbeddingJson = JsonSerializer.Serialize(chunk.Embedding)
                };

                _dbContext.DocumentChunks.Add(entity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<StoredDocumentChunk>>
            GetAllChunksAsync()
        {
            var entities = await _dbContext.DocumentChunks
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(entity =>
                new StoredDocumentChunk
                {
                    DocumentName = entity.DocumentName,
                    PageNumber = entity.PageNumber,
                    DocumentHash = entity.DocumentHash,
                    ChunkNumber = entity.ChunkNumber,
                    Content = entity.Content,
                    Embedding = JsonSerializer.Deserialize<float[]>(entity.EmbeddingJson) ?? Array.Empty<float>()
                }).ToList();
        }
        public async Task<bool> DocumentHashExistsAsync(string documentHash)
        {
            return await _dbContext.DocumentChunks
                .AnyAsync(chunk => chunk.DocumentHash == documentHash);
        }
    }
}
