using CineHub.Models.ViewModels.Base;
using CineHub.Models;

namespace CineHub.Models.ViewModels.Search
{
    public class SearchIndexViewModel : BaseViewModel
    {
        public List<Movie> Movies { get; set; } = new();
        public string SearchQuery { get; set; } = string.Empty;
        public int? MinRating { get; set; }
        public int? ReleaseYear { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public bool HasSearched { get; set; } = false;

        // Helper properties for display
        public bool HasResults => Movies.Any();
        public int ResultsCount => Movies.Count;

        // Helper method to check if any filter is active
        public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchQuery) || MinRating.HasValue || ReleaseYear.HasValue;
    }
}