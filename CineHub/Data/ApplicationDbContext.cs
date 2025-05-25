using Microsoft.EntityFrameworkCore;
using CineHub.Models;

namespace CineHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>()
                .HasIndex(m => m.TMDbId)
                .IsUnique();

            modelBuilder.Entity<Movie>()
               .Property(e => e.ReleaseDate)
               .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Movie>()
                .Property(e => e.LastUpdated)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}