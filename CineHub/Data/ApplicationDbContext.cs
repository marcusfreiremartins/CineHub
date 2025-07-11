using CineHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CineHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<MoviePerson> MoviePeople { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //Movie
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


            //User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<User>()
                .Property(e => e.LastLogin)
                .HasColumnType("timestamp without time zone");

            //UserRating
            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => new { ur.UserId, ur.MovieId })
                .IsUnique();

            modelBuilder.Entity<UserRating>()
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<UserRating>()
                .Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<UserRating>()
                .Property(e => e.LastActivityDate)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.Movie)
                .WithMany()
                .HasForeignKey(ur => ur.MovieId)
                .OnDelete(DeleteBehavior.Cascade);


            //UserFavorite
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(uf => new { uf.UserId, uf.MovieId })
                .IsUnique();

            modelBuilder.Entity<UserFavorite>()
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Movie)
                .WithMany()
                .HasForeignKey(uf => uf.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Person
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.TMDbId)
                .IsUnique();

            modelBuilder.Entity<Person>()
                .Property(e => e.Birthday)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Person>()
                .Property(e => e.Deathday)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Person>()
                .Property(e => e.LastUpdated)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // MoviePerson
            modelBuilder.Entity<MoviePerson>()
                .HasKey(mp => new { mp.MovieId, mp.PersonId, mp.Role });

            modelBuilder.Entity<MoviePerson>()
                .HasOne(mp => mp.Movie)
                .WithMany()
                .HasForeignKey(mp => mp.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MoviePerson>()
                .HasOne(mp => mp.Person)
                .WithMany()
                .HasForeignKey(mp => mp.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}