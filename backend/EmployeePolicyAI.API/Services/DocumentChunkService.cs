using EmployeePolicyAI.API.Models;

namespace EmployeePolicyAI.API.Services
{
    public class DocumentChunkService
    {
        private const int ChunkSize = 500;
        private const int Overlap = 100;

        public List<DocumentChunk> CreateChunks(
            string documentName,
            string text)
        {
            var chunks = new List<DocumentChunk>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return chunks;
            }

            int start = 0;
            int chunkNumber = 1;

            while (start < text.Length)
            {
                int length = Math.Min(
                    ChunkSize,
                    text.Length - start);

                string content = text.Substring(start, length);

                chunks.Add(new DocumentChunk
                {
                    DocumentName = documentName,
                    PageNumber = 0,
                    ChunkNumber = chunkNumber,
                    Content = content
                });

                chunkNumber++;

                if (start + length >= text.Length)
                {
                    break;
                }

                start += ChunkSize - Overlap;
            }

            return chunks;
        }

        public List<DocumentChunk> CreatePageChunks(
            string documentName,
            List<PdfPageText> pages)
        {
            var allChunks = new List<DocumentChunk>();
            int chunkNumber = 1;

            foreach (var page in pages)
            {
                if (string.IsNullOrWhiteSpace(page.Text))
                {
                    continue;
                }

                int start = 0;

                while (start < page.Text.Length)
                {
                    int length = Math.Min(
                        ChunkSize,
                        page.Text.Length - start);

                    string content = page.Text.Substring(start, length);

                    allChunks.Add(new DocumentChunk
                    {
                        DocumentName = documentName,
                        PageNumber = page.PageNumber,
                        ChunkNumber = chunkNumber,
                        Content = content
                    });

                    chunkNumber++;

                    if (start + length >= page.Text.Length)
                    {
                        break;
                    }

                    start += ChunkSize - Overlap;
                }
            }

            return allChunks;
        }
    }
}
