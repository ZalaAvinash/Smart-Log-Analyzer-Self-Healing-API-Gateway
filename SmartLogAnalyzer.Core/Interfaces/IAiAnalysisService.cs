using SmartLogAnalyzer.Core.Models;

namespace SmartLogAnalyzer.Core.Interfaces
{
    public interface IAiAnalysisService
    {
        Task<ErrorLog> AnalyzeErrorAsync(ErrorLog errorLog);
    }
}