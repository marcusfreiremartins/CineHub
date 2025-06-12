using CineHub.Models.ViewModels.Base;
namespace CineHub.Models.ViewModels.Movies
{
    public class MovieCommentsViewModel : BaseViewModel
    {
        public List<MovieCommentItemViewModel> Comments { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalComments { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
    }

    public class MovieCommentItemViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("dd/MM/yyyy") ?? "";
        public bool WasUpdated => UpdatedAt.HasValue;
        public string RatingDisplay => GetNumericRating(Rating);
        private string GetNumericRating(int rating)
        {
            return $"{rating}/10";
        }
    }
}