namespace CineHub.Models.ViewModels.User
{
    public class RatingItemViewModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePosterPath { get; set; } = string.Empty;
        public string MovieOverview { get; set; } = string.Empty;
        public int UserRating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FormattedCreatedDate => CreatedAt.ToString("dd/MM/yyyy");
        public string FormattedUpdatedDate => UpdatedAt?.ToString("dd/MM/yyyy") ?? "";
        public bool WasUpdated => UpdatedAt.HasValue;
    }
}