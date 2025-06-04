using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.User
{
    public class UserRatingsViewModel : BaseViewModel
    {
        public List<RatingItemViewModel> Ratings { get; set; } = new();
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}