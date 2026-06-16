using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLogAnalyzer.Infrastructure.Data;

namespace SmartLogAnalyzer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ErrorLogDbContext _context;

        public ErrorController(ErrorLogDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetErrors()
        {
            var errors = await _context.ErrorLogs.OrderByDescending(e => e.Timestamp).ToListAsync();
            return Ok(errors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetError(int id)
        {
            var error = await _context.ErrorLogs.FindAsync(id);
            if (error == null) return NotFound();
            return Ok(error);
        }
    }
}