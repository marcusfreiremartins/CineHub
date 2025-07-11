using System.ComponentModel.DataAnnotations;
using CineHub.Models.ViewModels.Base;

namespace CineHub.Models.ViewModels.Movies
{
    public class RateMovieViewModel : BaseViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePosterPath { get; set; } = string.Empty;
        public string? MovieBackdropPath { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "A nota deve ser entre 1 e 10")]
        public int Rating { get; set; }

        [StringLength(8000, ErrorMessage = "O comentário deve ter no máximo 8000 caracteres")]
        public string? Comment { get; set; }

        public bool IsFavorite { get; set; }
        public int? UserRatingId { get; set; }
        public string? ReturnUrl { get; set; }
    }
}