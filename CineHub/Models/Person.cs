using System.ComponentModel.DataAnnotations;

namespace CineHub.Models
{
    public class Person
    {
        public int Id { get; set; }
        public int TMDbId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public string? ProfilePath { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? Deathday { get; set; }
        public string? PlaceOfBirth { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}