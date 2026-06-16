namespace SmartLogAnalyzer.Core.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string RoutePath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int OccurrenceCount { get; set; } = 1;
        public string? AiRootCause { get; set; }
        public string? AiFixSuggestion { get; set; }
        public string? AiCodePatch { get; set; }
    }
}