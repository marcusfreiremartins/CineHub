using System.ComponentModel.DataAnnotations;

namespace CineHub.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(200, ErrorMessage = "O email deve ter no máximo 200 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

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

    public class RateMovieViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterPath { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "A nota deve ser entre 1 e 10")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "O comentário deve ter no máximo 1000 caracteres")]
        public string? Comment { get; set; }

        public bool IsFavorite { get; set; }
        public int? ExistingRatingId { get; set; }
        public string ImageBaseUrl { get; set; } = string.Empty;
    }
}
