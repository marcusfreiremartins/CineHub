using CineHub.Extensions;
using CineHub.Models.Enum;

namespace CineHub.Models.ViewModels.Persons
{
    public class MoviePersonDetails
    {
        public Movie Movie { get; set; } = null!;
        public PersonRole Role { get; set; }
        public string? Character { get; set; }
        public int Order { get; set; }
        public string MovieTitle => Movie.Title;
        public string MoviePosterPath => Movie.PosterPath ?? string.Empty;
        public string MovieBackdropPath => Movie.BackdropPath ?? string.Empty;
        public string ReleaseYear => Movie.ReleaseDate?.ToString("yyyy") ?? "N/A";
        public string FormattedReleaseDate => Movie.ReleaseDate?.ToString("dd/MM/yyyy") ?? "Data não disponível";
        public string RoleDisplayName => Role.GetDisplayName();

    }
}