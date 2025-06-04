using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.User
{
    public class UserProfileViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalRatings { get; set; }
        public int TotalFavorites { get; set; }
        public double AverageRating { get; set; }
        public List<UserRating> RecentRatings { get; set; } = new();
        public List<UserFavorite> RecentFavorites { get; set; } = new();
    }
}