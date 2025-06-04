namespace CineHub.Models.ViewModels.User
{
    public class FavoriteItemViewModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterPath { get; set; } = string.Empty;
        public string MovieOverview { get; set; } = string.Empty;
        public double MovieRating { get; set; }
        public DateTime AddedAt { get; set; }
        public string FormattedAddedDate => AddedAt.ToString("dd/MM/yyyy");
    }
}