using Microsoft.EntityFrameworkCore;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Core.Models;
using SmartLogAnalyzer.Infrastructure.Data;

namespace SmartLogAnalyzer.Infrastructure.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly ErrorLogDbContext _context;

        public ErrorLogRepository(ErrorLogDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorLog> AddOrUpdateErrorLogAsync(ErrorLog errorLog)
        {
            var existingLog = await _context.ErrorLogs
                .FirstOrDefaultAsync(e => e.StackTrace == errorLog.StackTrace);

            if (existingLog != null)
            {
                existingLog.OccurrenceCount++;
                existingLog.Timestamp = DateTime.UtcNow;
                _context.ErrorLogs.Update(existingLog);
                await _context.SaveChangesAsync();
                return existingLog;
            }
            else
            {
                await _context.ErrorLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();
                return errorLog;
            }
        }

        public async Task<ErrorLog?> GetErrorLogByStackTraceHashAsync(string stackTraceHash)
        {
            // In a real scenario, we would store the hash. 
            // For now, we assume the hash is the stack trace itself or we search by stack trace.
            // To optimize, we should store the hash in the DB.
            // Let's assume for now the 'stackTraceHash' is the stack trace for simplicity or we add a Hash column.
            // I will add a Hash column to the model later if needed, but for now let's just search by StackTrace.
            return await _context.ErrorLogs
                .FirstOrDefaultAsync(e => e.StackTrace == stackTraceHash);
        }
    }
}