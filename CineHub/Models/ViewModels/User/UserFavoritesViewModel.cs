using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.User
{
    public class UserFavoritesViewModel : BaseViewModel
    {
        public List<FavoriteItemViewModel> Favorites { get; set; } = new();
        public int TotalFavorites { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}