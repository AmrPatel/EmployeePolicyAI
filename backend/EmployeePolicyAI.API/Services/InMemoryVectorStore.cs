using EmployeePolicyAI.API.Models;

namespace EmployeePolicyAI.API.Services
{
    public class InMemoryVectorStore
    {
        private readonly List<StoredDocumentChunk> _chunks = new();

        private readonly object _lock = new();

        public void AddChunk(StoredDocumentChunk chunk)
        {
            lock (_lock)
            {
                _chunks.Add(chunk);
            }
        }

        public List<StoredDocumentChunk> GetAllChunks()
        {
            lock (_lock)
            {
                return _chunks.ToList();
            }
        }

        public List<(StoredDocumentChunk Chunk, float Score)> SearchWithScores(
    float[] queryEmbedding,
    int topK = 3)
        {
            if (topK <= 0)
            {
                return new List<(StoredDocumentChunk, float)>();
            }

            lock (_lock)
            {
                return _chunks
                    .Select(chunk => (
                        Chunk: chunk,
                        Score: CosineSimilarity(
                            queryEmbedding,
                            chunk.Embedding)))
                    .Where(item => item.Score >= 0)
                    .OrderByDescending(item => item.Score)
                    .Take(topK)
                    .ToList();
            }
        }

        public List<StoredDocumentChunk> Search(
            float[] queryEmbedding,
            int topK = 3)
        {
            if (topK <= 0)
            {
                return new List<StoredDocumentChunk>();
            }

            lock (_lock)
            {
                return _chunks
                    .Select(chunk => new
                    {
                        Chunk = chunk,
                        Score = CosineSimilarity(
                            queryEmbedding,
                            chunk.Embedding)
                    })
                    .OrderByDescending(item => item.Score)
                    .Take(topK)
                    .Select(item => item.Chunk)
                    .ToList();
            }
        }
        
        public void LoadChunks(List<StoredDocumentChunk> chunks)
        {
            lock (_lock)
            {
                _chunks.Clear();
                _chunks.AddRange(chunks);
            }
        }


        private static float CosineSimilarity(
            float[] vectorA,
            float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length ||
                vectorA.Length == 0)
            {
                return -1;
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];

                magnitudeA += vectorA[i] * vectorA[i];

                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return -1;
            }

            return (float)(
                dotProduct /
                (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB)));
        }
    }
}