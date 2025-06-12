using Microsoft.EntityFrameworkCore;
using CineHub.Data;
using CineHub.Models;

namespace CineHub.Services
{
    public class RatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Rates a movie by a user, adding or updating the rating and optional comment
        public async Task<(bool Success, string Message)> RateMovieAsync(int userId, int movieId, int rating, string? comment = null)
        {
            if (rating < 1 || rating > 10)
            {
                return (false, "A avaliação deve ser entre 1 e 10.");
            }

            try
            {
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

                if (existingRating != null)
                {
                    existingRating.Rating = rating;
                    existingRating.Comment = comment;
                    existingRating.UpdatedAt = DateTime.UtcNow;
                    existingRating.LastActivityDate = DateTime.UtcNow;
                    existingRating.DeletionDate = null;
                }
                else
                {
                    var newRating = new UserRating
                    {
                        UserId = userId,
                        MovieId = movieId,
                        Rating = rating,
                        Comment = comment,
                        CreatedAt = DateTime.UtcNow,
                        LastActivityDate = DateTime.UtcNow

                    };
                    _context.UserRatings.Add(newRating);
                }

                await _context.SaveChangesAsync();
                return (true, existingRating != null ? "Avaliação atualizada com sucesso!" : "Avaliação adicionada com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao salvar avaliação: {ex.Message}");
            }
        }

        // Retrieves a user's rating for a specific movie, if it exists
        public async Task<UserRating?> GetUserRatingAsync(int userId, int movieId)
        {
            return await _context.UserRatings
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId && r.DeletionDate == null);
        }

        // Toggles the favorite status silently for a movie without returning a message
        public async Task ToggleFavoriteSilentlyAsync(int userId, int movieId, bool isfavorite)
        {
            try
            {
                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

                if (existingFavorite != null && isfavorite == false)
                {
                    existingFavorite.DeletionDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return;
                }
                if (existingFavorite != null && existingFavorite.DeletionDate != null && isfavorite == true)
                {
                    existingFavorite.DeletionDate = null;
                    await _context.SaveChangesAsync();
                    return;
                }

                if (isfavorite == true && existingFavorite == null)
                {
                    var newFavorite = new UserFavorite
                    {
                        UserId = userId,
                        MovieId = movieId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserFavorites.Add(newFavorite);
                    await _context.SaveChangesAsync();
                    return;
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Toggles a movie as favorite or not, returning a success message
        public async Task<(bool Success, string Message)> ToggleFavoriteAsync(int userId, int movieId)
        {
            try
            {
                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

                if (existingFavorite != null && existingFavorite.DeletionDate == null)
                {
                    existingFavorite.DeletionDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return (true, "Filme removido dos favoritos!");
                }
                else
                {
                    var newFavorite = new UserFavorite
                    {
                        UserId = userId,
                        MovieId = movieId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserFavorites.Add(newFavorite);
                    await _context.SaveChangesAsync();
                    return (true, "Filme adicionado aos favoritos!");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Checks if a movie is marked as favorite by a user
        public async Task<bool> IsMovieFavoriteAsync(int userId, int movieId)
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.MovieId == movieId && f.DeletionDate == null);
        }

        // Retrieves a list of all favorite movies for a user
        public async Task<List<UserFavorite>> GetUserFavoritesAsync(int userId)
        {
            return await _context.UserFavorites
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId && f.DeletionDate == null)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        // Retrieves recent user ratings limited by the specified count
        public async Task<List<UserRating>> GetUserRatingsAsync(int userId, int take = 10)
        {
            return await _context.UserRatings
                .Include(r => r.Movie)
                .Where(r => r.UserId == userId && r.DeletionDate == null)
                .OrderByDescending(r => r.LastActivityDate)
                .Take(take)
                .ToListAsync();
        }

        // Calculates the average rating and total number of ratings given by a user
        public async Task<(double AverageRating, int TotalRatings)> GetUserRatingStatsAsync(int userId)
        {
            var ratings = await _context.UserRatings
                .Where(r => r.UserId == userId && r.DeletionDate == null)
                .Select(r => r.Rating)
                .ToListAsync();

            return (
                ratings.Any() ? ratings.Average() : 0,
                ratings.Count
            );
        }

        // Deletes a user's rating for a movie and optionally removes it from favorites
        public async Task<(bool Success, string Message)> DeleteRating(int userId, int movieId)
        {
            try
            {
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId && r.DeletionDate == null);

                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId && r.DeletionDate == null);

                if (existingRating != null && existingFavorite == null)
                {
                    existingRating.DeletionDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return (true, "Avaliação deletada com sucesso!");
                }
                if (existingFavorite != null && existingRating != null)
                {
                    existingFavorite.DeletionDate = DateTime.UtcNow;
                    existingRating.DeletionDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return (true, "Avaliação deletada e removido dos favoritos");
                }
                else
                {
                    return (false, $"Erro ao deletar Avaliação");
                }

            }
            catch (Exception ex)
            {
                return (false, $"Erro ao deletar avaliação: {ex.Message}");
            }
        }

        // Retrieves paginated comments for a specific movie, prioritizing those with comments
        public async Task<(List<UserRating> Comments, int TotalCount)> GetMovieCommentsAsync(int movieId, int page = 1, int pageSize = 5)
        {
            var query = _context.UserRatings
                .Include(r => r.User)
                .Where(r => r.MovieId == movieId && r.DeletionDate == null);

            var orderedQuery = query
                .OrderByDescending(r => !string.IsNullOrEmpty(r.Comment))
                .ThenByDescending(r => r.LastActivityDate);

            var totalCount = await orderedQuery.CountAsync();

            var comments = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (comments, totalCount);
        }

        // Gets movie rating statistics (average rating and total count)
        public async Task<(double AverageRating, int TotalRatings)> GetMovieRatingStatsAsync(int movieId)
        {
            var ratings = await _context.UserRatings
                .Where(r => r.MovieId == movieId && r.DeletionDate == null)
                .Select(r => r.Rating)
                .ToListAsync();

            return (
                ratings.Any() ? ratings.Average() : 0,
                ratings.Count
            );
        }
    }
}