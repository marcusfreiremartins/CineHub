namespace CineHub.Models.ViewModels
{
    public class UserFavoritesViewModel : BaseViewModel
    {
        public List<FavoriteItemViewModel> Favorites { get; set; } = new();
        public int TotalFavorites { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class FavoriteItemViewModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterPath { get; set; } = string.Empty;
        public string MovieOverview { get; set; } = string.Empty;
        public double MovieRating { get; set; }
        public string? PosterPath { get; set; }
        public DateTime AddedAt { get; set; }
        public string FormattedAddedDate => AddedAt.ToString("dd/MM/yyyy");
    }

    public class UserRatingsViewModel : BaseViewModel
    {
        public List<RatingItemViewModel> Ratings { get; set; } = new();
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class RatingItemViewModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterPath { get; set; } = string.Empty;
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