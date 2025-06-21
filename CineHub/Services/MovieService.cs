using Microsoft.EntityFrameworkCore;
using CineHub.Data;
using CineHub.Models;
using CineHub.Models.DTOs;

namespace CineHub.Services
{
    public class MovieService
    {
        private readonly ApplicationDbContext _context;
        private readonly TMDbService _tmdbService;

        public MovieService(ApplicationDbContext context, TMDbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        // Retrieves popular movies. First tries TMDb API; falls back to local DB if the API is unavailable
        public async Task<List<Movie>> GetPopularMoviesAsync(int page = 1)
        {
            try
            {
                var moviesFromApi = await _tmdbService.GetPopularMoviesAsync(page);

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi)
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable; falling back to local DB: {ex.Message}");
            }

            // Fallback: Fetch popular movies from local DB
            return await GetPopularMoviesFromDatabaseAsync(page);
        }

        // Retrieves a movie by its local DB ID
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        // Retrieves a movie by its TMDb ID. Tries API first; falls back to local DB
        public async Task<Movie?> GetMovieByTMDbIdAsync(int tmdbId)
        {
            try
            {
                var movieDto = await _tmdbService.GetMovieDetailsAsync(tmdbId);
                if (movieDto != null)
                {
                    return await GetOrCreateMovieAsync(movieDto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for movie details; falling back to DB: {ex.Message}");
            }

            return await _context.Movies.FirstOrDefaultAsync(m => m.TMDbId == tmdbId);
        }

        // Searches movies by title. First tries API; falls back to DB
        public async Task<List<Movie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Movie>();

            try
            {
                var moviesFromApi = await _tmdbService.SearchMoviesAsync(query, page);

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi.Take(20))
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for search; falling back to DB: {ex.Message}");
            }

            return await SearchMoviesInDatabaseAsync(query, page);
        }

        // Fetches top-rated movies. Falls back to DB if API fails
        public async Task<List<Movie>> GetTopRatedMoviesAsync()
        {
            try
            {
                var moviesFromApi = await _tmdbService.GetTopRatedMoviesAsync();

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi)
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for top-rated movies; falling back to DB: {ex.Message}");
            }

            return await GetTopRatedMoviesFromDatabaseAsync();
        }

        // Advanced search with optional title, rating, and year filters
        public async Task<List<Movie>> AdvancedSearchAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1)
        {
            try
            {
                List<MovieDTO> moviesFromApi;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    moviesFromApi = await _tmdbService.SearchMoviesWithFiltersAsync(
                        query, releaseYear, minRating.HasValue ? (double)minRating.Value : null, page);
                }
                else if (releaseYear.HasValue && minRating.HasValue)
                {
                    moviesFromApi = await _tmdbService.DiscoverMoviesAsync(
                        releaseYear.Value, (double)minRating.Value, page);
                }
                else if (releaseYear.HasValue)
                {
                    moviesFromApi = await _tmdbService.GetMoviesByYearAsync(releaseYear.Value, page);
                }
                else if (minRating.HasValue)
                {
                    moviesFromApi = await _tmdbService.GetMoviesByMinRatingAsync((double)minRating.Value, page);
                }
                else
                {
                    moviesFromApi = await _tmdbService.GetPopularMoviesAsync(page);
                }

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();
                    foreach (var movieDto in moviesFromApi.Take(20))
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for advanced search; falling back to DB: {ex.Message}");
            }

            return await AdvancedSearchInDatabaseAsync(query, minRating, releaseYear, page);
        }

        // Fetch movies by year. Falls back to DB if API fails
        public async Task<List<Movie>> GetMoviesByYearAsync(int year, int page = 1)
        {
            try
            {
                var moviesFromApi = await _tmdbService.GetMoviesByYearAsync(year, page);

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi)
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for movies by year; falling back to DB: {ex.Message}");
            }

            return await _context.Movies
                .Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == year)
                .OrderByDescending(m => m.VoteAverage)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();
        }

        // Fetch movies by minimum rating. Falls back to DB if API fails
        public async Task<List<Movie>> GetMoviesByMinRatingAsync(int minRating, int page = 1)
        {
            try
            {
                var moviesFromApi = await _tmdbService.GetMoviesByMinRatingAsync(minRating, page);

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi)
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    return movies;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for movies by rating; falling back to DB: {ex.Message}");
            }

            return await _context.Movies
                .Where(m => (int)Math.Round(m.VoteAverage) >= minRating)
                .OrderByDescending(m => m.VoteAverage)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();
        }

        #region Database Fallback Methods

        // Fetch popular movies from DB
        private async Task<List<Movie>> GetPopularMoviesFromDatabaseAsync(int page = 1)
        {
            return await _context.Movies
                .OrderByDescending(m => m.VoteAverage)
                .ThenByDescending(m => m.VoteCount)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();
        }

        // Search movies in DB by title or overview
        private async Task<List<Movie>> SearchMoviesInDatabaseAsync(string query, int page = 1)
        {
            return await _context.Movies
                .Where(m => m.Title.ToLower().Contains(query.ToLower()) ||
                            m.Overview.ToLower().Contains(query.ToLower()))
                .OrderByDescending(m => m.VoteAverage)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();
        }

        // Fetch top-rated movies from DB
        private async Task<List<Movie>> GetTopRatedMoviesFromDatabaseAsync()
        {
            return await _context.Movies
                .Where(m => m.VoteAverage >= 8.0)
                .OrderByDescending(m => m.VoteAverage)
                .Take(20)
                .ToListAsync();
        }

        // Advanced search in DB with optional title, rating, and year filters
        private async Task<List<Movie>> AdvancedSearchInDatabaseAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1)
        {
            var moviesQuery = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                moviesQuery = moviesQuery.Where(m =>
                    m.Title.ToLower().Contains(query.ToLower()) ||
                    m.Overview.ToLower().Contains(query.ToLower()));
            }

            if (releaseYear.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);
            }

            if (minRating.HasValue)
            {
                moviesQuery = moviesQuery.Where(m => (int)Math.Round(m.VoteAverage) >= minRating.Value);
            }

            return await moviesQuery
                .OrderByDescending(m => m.VoteAverage)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();
        }

        #endregion

        // Retrieves or creates a movie in the DB
        private async Task<Movie?> GetOrCreateMovieAsync(MovieDTO movieDto)
        {
            var existingMovie = await _context.Movies
                .FirstOrDefaultAsync(m => m.TMDbId == movieDto.Id);

            if (existingMovie != null)
            {
                var shouldUpdate = (DateTime.UtcNow - existingMovie.LastUpdated).TotalDays > 7;
                if (shouldUpdate)
                {
                    existingMovie.Title = movieDto.Title;
                    existingMovie.Overview = movieDto.Overview;
                    existingMovie.ReleaseDate = DateTime.TryParse(movieDto.ReleaseDate, out var date)
                        ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                        : existingMovie.ReleaseDate;
                    existingMovie.PosterPath = movieDto.PosterPath;
                    existingMovie.VoteAverage = movieDto.VoteAverage;
                    existingMovie.VoteCount = movieDto.VoteCount;
                    existingMovie.LastUpdated = DateTime.UtcNow;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while updating cached movie: {ex.Message}");
                    }
                }

                return existingMovie;
            }

            var newMovie = new Movie
            {
                TMDbId = movieDto.Id,
                Title = movieDto.Title,
                Overview = movieDto.Overview,
                ReleaseDate = DateTime.TryParse(movieDto.ReleaseDate, out var parsedDate)
                    ? DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc)
                    : null,
                PosterPath = movieDto.PosterPath,
                VoteAverage = movieDto.VoteAverage,
                VoteCount = movieDto.VoteCount,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                _context.Movies.Add(newMovie);
                await _context.SaveChangesAsync();
                return newMovie;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving new movie to DB: {ex.Message}");
                return null;
            }
        }
    }
}