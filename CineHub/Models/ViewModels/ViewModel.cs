namespace CineHub.Models.ViewModels
{
    public abstract class BaseViewModel
    {
        public string ImageBaseUrl { get; set; } = string.Empty;
    }

    public class MovieIndexViewModel : BaseViewModel
    {
        public List<Movie> Movies { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }

    public class MovieDetailsViewModel : BaseViewModel
    {
        public Movie Movie { get; set; } = new();
    }
}
