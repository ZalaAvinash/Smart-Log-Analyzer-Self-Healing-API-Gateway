using Microsoft.AspNetCore.Mvc;

namespace SmartLogAnalyzer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriggerErrorController : ControllerBase
    {
        [HttpGet]
        public IActionResult Trigger()
        {
            throw new InvalidOperationException("This is a test exception to demonstrate the Smart Log Analyzer.");
        }
    }
}