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
                var jsonDoc = JsonDocument.Parse(responseText);
                var root = jsonDoc.RootElement;

                errorLog.AiRootCause = root.GetProperty("RootCause").GetString();
                errorLog.AiFixSuggestion = root.GetProperty("FixSuggestion").GetString();
                errorLog.AiCodePatch = root.GetProperty("CodePatch").GetString();
            }
            catch
            {
                errorLog.AiRootCause = "Failed to parse AI response.";
                errorLog.AiFixSuggestion = responseText;
                errorLog.AiCodePatch = "N/A";
            }

            return errorLog;
        }
    }
}