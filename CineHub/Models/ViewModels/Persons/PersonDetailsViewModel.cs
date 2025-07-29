using CineHub.Models;
using CineHub.Models.Enum;
using CineHub.Models.ViewModels.Base;
using CineHub.Extensions;

namespace CineHub.Models.ViewModels.Persons
{
    public class PersonDetailsViewModel : BaseViewModel
    {
        public Person Person { get; set; } = null!;
        public List<MoviePersonDetails> Movies { get; set; } = new();

        // Main property: movies grouped by movie
        public List<GroupedMovieDetails> GroupedMovies => Movies
            .GroupBy(m => m.Movie.Id)
            .Select(g => new GroupedMovieDetails
            {
                Movie = g.First().Movie,
                Roles = g.Select(r => new MovieRole
                {
                    Role = r.Role,
                    Character = r.Character,
                    Order = r.Order
                }).OrderBy(r => r.Role.GetImportance()).ToList()
            })
            .OrderByDescending(gm => gm.Movie.ReleaseDate)
            .ToList();

        // General statistics
        public int TotalMovies => GroupedMovies.Count;
        public int TotalRoles => Movies.Count;

        // Statistics by role type
        public int TotalActingRoles => Movies.Count(m => m.Role.IsActorRole());
        public int TotalDirectingRoles => Movies.Count(m => m.Role.IsDirectorRole());
        public int TotalWritingRoles => Movies.Count(m => m.Role.IsWriterRole());
        public int TotalProducingRoles => Movies.Count(m => m.Role.IsProducerRole());
        public int TotalOtherRoles => Movies.Count(m => !m.Role.IsMainRole());
    }

    public class GroupedMovieDetails
    {
        public Movie Movie { get; set; } = null!;
        public List<MovieRole> Roles { get; set; } = new();

        // Convenience properties
        public string MovieTitle => Movie.Title;
        public string MoviePosterPath => Movie.PosterPath ?? string.Empty;
        public string ReleaseYear => Movie.ReleaseDate?.ToString("yyyy") ?? "N/A";
        public string FormattedReleaseDate => Movie.ReleaseDate?.ToString("dd/MM/yyyy") ?? "Data não disponível";
        public string FormattedRating => Movie.VoteAverage > 0 ? Movie.VoteAverage.ToString("F1") : "N/A";
        public bool HasPoster => !string.IsNullOrEmpty(Movie.PosterPath);
        public bool HasReleaseDate => Movie.ReleaseDate.HasValue;

        // Information about the roles
        public bool HasMultipleRoles => Roles.Count > 1;
        public MovieRole PrimaryRole => Roles.OrderBy(r => r.Role.GetImportance()).First();

        public string RolesDisplay
        {
            get
            {
                if (!Roles.Any()) return "N/A";

                var roleDescriptions = Roles.Select(r =>
                {
                    var roleDisplay = r.Role.GetDisplayName();
                    return !string.IsNullOrEmpty(r.Character) && r.Role.IsActorRole()
                        ? $"{roleDisplay} (como {r.Character})"
                        : roleDisplay;
                }).ToList();

                return string.Join(", ", roleDescriptions);
            }
        }

        public string ShortRolesDisplay
        {
            get
            {
                if (!Roles.Any()) return "N/A";

                if (Roles.Count == 1)
                {
                    var role = Roles.First();
                    return !string.IsNullOrEmpty(role.Character) && role.Role.IsActorRole()
                        ? $"como {role.Character}"
                        : role.Role.GetDisplayName();
                }

                return $"{Roles.Count} papéis";
            }
        }

        // Role type checks
        public bool HasActingRole => Roles.Any(r => r.Role.IsActorRole());
        public bool HasDirectingRole => Roles.Any(r => r.Role.IsDirectorRole());
        public bool HasWritingRole => Roles.Any(r => r.Role.IsWriterRole());
        public bool HasProducingRole => Roles.Any(r => r.Role.IsProducerRole());
    }

    public class MovieRole
    {
        public PersonRole Role { get; set; }
        public string? Character { get; set; }
        public int Order { get; set; }

        public string RoleDisplayName => Role.GetDisplayName();
        public bool IsActingRole => Role.IsActorRole();
        public bool HasCharacter => !string.IsNullOrEmpty(Character);

        public string DisplayText
        {
            get
            {
                if (!string.IsNullOrEmpty(Character) && Role.IsActorRole())
                {
                    return $"{RoleDisplayName} (como {Character})";
                }
                return RoleDisplayName;
            }
        }
    }
}