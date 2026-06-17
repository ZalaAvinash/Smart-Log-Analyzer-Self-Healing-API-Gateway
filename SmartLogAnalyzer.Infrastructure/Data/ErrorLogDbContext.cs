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
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.Property(e => e.StackTrace)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AiRootCause)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AiFixSuggestion)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AiCodePatch)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ErrorMessage)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.RoutePath)
                    .HasMaxLength(500);
            });
        }
    }
}
