using EmployeePolicyAI.API.Models;
using UglyToad.PdfPig;

namespace EmployeePolicyAI.API.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task<string> ExtractTextFromPdfAsync(
            string filePath)
        {
            return await Task.Run(() =>
            {
                using var document =
                    PdfDocument.Open(filePath);

                var text = new System.Text.StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return text.ToString();
            });
        }
        public Task<List<PdfPageText>> ExtractPagesFromPdfAsync(
            string filePath)
        {
            var pages = new List<PdfPageText>();

            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    pages.Add(new PdfPageText
                    {
                        PageNumber = page.Number,
                        Text = page.Text
                    });
                }
            }

            return Task.FromResult(pages);
        }
    }
}
