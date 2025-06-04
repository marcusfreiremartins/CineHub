using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Movies
{
    public class MovieDetailsViewModel : BaseViewModel
    {
        public Movie Movie { get; set; } = new();
    }
}