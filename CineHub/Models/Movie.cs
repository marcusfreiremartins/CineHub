using System.ComponentModel.DataAnnotations;

namespace CineHub.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public int TMDbId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public string? PosterPath { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}