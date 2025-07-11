using CineHub.Models.Enum;

namespace CineHub.Models
{
    public class MoviePerson
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
        public PersonRole Role { get; set; }
        public string? Character { get; set; }
        public int Order { get; set; }
    }
}