using CineHub.Models.Enum;
using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Movies
{
    public class MovieDetailsViewModel : BaseViewModel
    {
        public Movie Movie { get; set; } = new();
        public MovieCommentsViewModel Comments { get; set; } = new();
        public double AverageUserRating { get; set; }
        public int TotalUserRatings { get; set; }
        public string FormattedAverageRating => AverageUserRating > 0 ? AverageUserRating.ToString("F1") : "0.0";
        public List<(Person Person, PersonRole Role, string? Character)> Credits { get; set; } = new();
    }
}