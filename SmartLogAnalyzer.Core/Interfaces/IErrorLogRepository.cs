using SmartLogAnalyzer.Core.Models;

namespace SmartLogAnalyzer.Core.Interfaces
{
    public interface IErrorLogRepository
    {
        Task<ErrorLog> AddOrUpdateErrorLogAsync(ErrorLog errorLog);
        Task<ErrorLog?> GetErrorLogByStackTraceHashAsync(string stackTraceHash);
    }
}