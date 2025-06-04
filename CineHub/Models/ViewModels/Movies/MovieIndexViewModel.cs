using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Movies
{
    public class MovieIndexViewModel : BaseViewModel
    {
        public List<Movie> Movies { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }
}