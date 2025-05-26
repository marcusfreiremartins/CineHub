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
                }
                else
                {
                    var newRating = new UserRating
                    {
                        UserId = userId,
                        MovieId = movieId,
                        Rating = rating,
                        Comment = comment,
                        CreatedAt = DateTime.UtcNow
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

        public async Task<UserRating?> GetUserRatingAsync(int userId, int movieId)
        {
            return await _context.UserRatings
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId && r.DeletionDate == null);
        }

        //public async Task<List<UserRating>> GetMovieRatingsAsync(int movieId)
        //{
            //return await _context.UserRatings
                //.Include(r => r.User)
                //.Where(r => r.MovieId == movieId)
                //.OrderByDescending(r => r.CreatedAt)
                //.ToListAsync();
        //}

        //public async Task<double> GetMovieAverageRatingAsync(int movieId)
        //{
            //var ratings = await _context.UserRatings
                //.Where(r => r.MovieId == movieId)
                //.Select(r => r.Rating)
                //.ToListAsync();

            //return ratings.Any() ? ratings.Average() : 0;
        //}

        //public async Task<int> GetMovieRatingCountAsync(int movieId)
        //{
            //return await _context.UserRatings
                //.CountAsync(r => r.MovieId == movieId);
        //}

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
            catch (Exception ex)
            {
                throw ex;
            }
        }


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
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar favoritos: {ex.Message}");
            }
        }

        public async Task<bool> IsMovieFavoriteAsync(int userId, int movieId)
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.MovieId == movieId && f.DeletionDate == null);
        }

        public async Task<List<UserFavorite>> GetUserFavoritesAsync(int userId)
        {
            return await _context.UserFavorites
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId && f.DeletionDate == null)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserRating>> GetUserRatingsAsync(int userId, int take = 10)
        {
            return await _context.UserRatings
                .Include(r => r.Movie)
                .Where(r => r.UserId == userId && r.DeletionDate == null)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<(double AverageRating, int TotalRatings)> GetUserRatingStatsAsync(int userId)
        {
            var ratings = await _context.UserRatings
                .Where(r => r.UserId == userId)
                .Select(r => r.Rating)
                .ToListAsync();

            return (
                ratings.Any() ? ratings.Average() : 0,
                ratings.Count
            );
        }

        public async Task<(bool Success, string Message)> DeleteRating(int userId, int movieId)
        {
            try
            {
                var existingRating = await _context.UserRatings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);
                if (existingRating != null && existingRating.DeletionDate == null)
                {
                    existingRating.DeletionDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return (true, "Avaliação excluida");
                }
                else
                {
                    return (false, $"Erro ao excluir Avaliação");
                }

            }
            catch (Exception ex)
            {
                return (false, $"Erro ao excluir avaliação: {ex.Message}");
            }
        }
    }
}