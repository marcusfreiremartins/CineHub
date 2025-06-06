using System.ComponentModel.DataAnnotations;

namespace CineHub.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime? DeletionDate { get; set; }

        // Navigation properties
        public virtual ICollection<UserRating> Ratings { get; set; } = new List<UserRating>();
        public virtual ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
    }

    public class UserRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }

        [Range(1, 10)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletionDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Movie Movie { get; set; } = null!;
    }

    public class UserFavorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Movie Movie { get; set; } = null!;
        public DateTime? DeletionDate { get; set; }
    }
}