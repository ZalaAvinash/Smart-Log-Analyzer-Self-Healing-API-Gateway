using Microsoft.SemanticKernel;
using SmartLogAnalyzer.Core.Interfaces;
using SmartLogAnalyzer.Core.Models;
using System.Text.Json;

namespace SmartLogAnalyzer.Worker.Services
{
    public class AiAnalysisService : IAiAnalysisService
    {
        private readonly Kernel _kernel;

        public AiAnalysisService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<ErrorLog> AnalyzeErrorAsync(ErrorLog errorLog)
        {
            var prompt = $@"
            You are a Senior .NET Engineer. Analyze the following error and provide a JSON response with exactly three keys: RootCause, FixSuggestion, and CodePatch.
            
            Error Message: {errorLog.ErrorMessage}
            Stack Trace: {errorLog.StackTrace}
            
            JSON Response:
            ";

            var result = await _kernel.InvokePromptAsync(prompt);
            var responseText = result.ToString();

            try
            {
                // Clean up markdown code blocks if present
                var cleaned = responseText.Trim();
                if (cleaned.StartsWith("```json"))
                    cleaned = cleaned.Substring(7);
                if (cleaned.StartsWith("```"))
                    cleaned = cleaned.Substring(3);
                if (cleaned.EndsWith("```"))
                    cleaned = cleaned.Substring(0, cleaned.Length - 3);
                cleaned = cleaned.Trim();

                var jsonDoc = JsonDocument.Parse(cleaned);
                var root = jsonDoc.RootElement;

                errorLog.AiRootCause = root.GetProperty("RootCause").GetString();
                errorLog.AiFixSuggestion = root.GetProperty("FixSuggestion").GetString();
                errorLog.AiCodePatch = root.GetProperty("CodePatch").GetString();
            }
            catch
            {
                // Try to extract values using regex-like string parsing
                try
                {
                    errorLog.AiRootCause = ExtractJsonValue(responseText, "RootCause");
                    errorLog.AiFixSuggestion = ExtractJsonValue(responseText, "FixSuggestion");
                    errorLog.AiCodePatch = ExtractJsonValue(responseText, "CodePatch") ?? "N/A";
                }
                catch
                {
                    errorLog.AiRootCause = "Failed to parse AI response.";
                    errorLog.AiFixSuggestion = responseText.Length > 500 ? responseText.Substring(0, 500) + "..." : responseText;
                    errorLog.AiCodePatch = "N/A";
                }
            }

            return errorLog;
        }

        private string? ExtractJsonValue(string text, string key)
        {
            var pattern = $"\"{key}\"\\s*:\\s*\"";
            var startIndex = text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (startIndex < 0) return null;

            var valueStart = startIndex + pattern.Length;
            var endIndex = valueStart;
            var escaped = false;

            while (endIndex < text.Length)
            {
                if (escaped)
                {
                    escaped = false;
                    endIndex++;
                    continue;
                }
                if (text[endIndex] == '\\')
                {
                    escaped = true;
                    endIndex++;
                    continue;
                }
                if (text[endIndex] == '"')
                    break;
                endIndex++;
            }

            if (endIndex >= text.Length) return null;

            var value = text.Substring(valueStart, endIndex - valueStart);
            return value.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
