using EmployeePolicyAI.API.Models;

namespace EmployeePolicyAI.API.Services
{
    public interface IDocumentService
    {
        Task<string> ExtractTextFromPdfAsync(
            string filePath);

        Task<List<PdfPageText>> ExtractPagesFromPdfAsync(
            string filePath);
    }
}
