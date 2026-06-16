using Microsoft.EntityFrameworkCore;
using SmartLogAnalyzer.Core.Models;

namespace SmartLogAnalyzer.Infrastructure.Data
{
    public class ErrorLogDbContext : DbContext
    {
        public ErrorLogDbContext(DbContextOptions<ErrorLogDbContext> options) : base(options)
        {
        }

        public DbSet<ErrorLog> ErrorLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>()
                .HasIndex(e => e.StackTrace)
                .IsUnique();
        }
    }
}