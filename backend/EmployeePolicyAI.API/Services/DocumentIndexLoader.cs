using Microsoft.Extensions.Hosting;

namespace EmployeePolicyAI.API.Services
{
    public class DocumentIndexLoader : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DocumentIndexLoader> _logger;

        public DocumentIndexLoader(
            IServiceScopeFactory scopeFactory,
            ILogger<DocumentIndexLoader> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var repository = scope.ServiceProvider
                    .GetRequiredService<DocumentChunkRepository>();

                var vectorStore = scope.ServiceProvider
                    .GetRequiredService<InMemoryVectorStore>();

                var chunks = await repository.GetAllChunksAsync();

                vectorStore.LoadChunks(chunks);

                _logger.LogInformation(
                    "Loaded {Count} document chunks into vector search.",
                    chunks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load document chunks from SQL Server.");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

