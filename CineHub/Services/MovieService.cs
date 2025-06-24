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
        private const int DefaultPageSize = 20;

        public MovieService(ApplicationDbContext context, TMDbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        // Retrieves popular movies with pagination
        public async Task<PaginatedResult<Movie>> GetPopularMoviesAsync(int page = 1, int pageSize = DefaultPageSize)
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

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 10000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable; falling back to local DB: {ex.Message}");
            }

            return await GetPopularMoviesFromDatabaseAsync(page, pageSize);
        }

        // Method to fetch popular movies without pagination (for Home)
        public async Task<List<Movie>> GetPopularMoviesAsync()
        {
            var result = await GetPopularMoviesAsync(1, DefaultPageSize);
            return result.Items;
        }

        // Retrieves a movie by its local DB ID
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        // Retrieves a movie by its TMDb ID
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

        // Searches movies by title with pagination
        public async Task<PaginatedResult<Movie>> SearchMoviesAsync(string query, int page = 1, int pageSize = DefaultPageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new PaginatedResult<Movie>(new List<Movie>(), 0, page, pageSize);

            try
            {
                var moviesFromApi = await _tmdbService.SearchMoviesAsync(query, page);

                if (moviesFromApi?.Any() == true)
                {
                    var movies = new List<Movie>();

                    foreach (var movieDto in moviesFromApi.Take(pageSize))
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 1000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for search; falling back to DB: {ex.Message}");
            }

            return await SearchMoviesInDatabaseAsync(query, page, pageSize);
        }

        // Method to fetch movies without pagination (for Home)
        public async Task<List<Movie>> SearchMoviesAsync(string query)
        {
            var result = await SearchMoviesAsync(query, 1, DefaultPageSize);
            return result.Items;
        }

        // Fetches top-rated movies with pagination
        public async Task<PaginatedResult<Movie>> GetTopRatedMoviesAsync(int page = 1, int pageSize = DefaultPageSize)
        {
            try
            {
                var moviesFromApi = await _tmdbService.GetTopRatedMoviesAsync(page);

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

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 5000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for top-rated movies; falling back to DB: {ex.Message}");
            }

            return await GetTopRatedMoviesFromDatabaseAsync(page, pageSize);
        }

        // Method to fetch top-rated movies without pagination (for Home)
        public async Task<List<Movie>> GetTopRatedMoviesAsync()
        {
            var result = await GetTopRatedMoviesAsync(1, DefaultPageSize);
            return result.Items;
        }

        // Performs an advanced search with optional filters and pagination
        public async Task<PaginatedResult<Movie>> AdvancedSearchAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1, int pageSize = DefaultPageSize)
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
                    foreach (var movieDto in moviesFromApi.Take(pageSize))
                    {
                        var movie = await GetOrCreateMovieAsync(movieDto);
                        if (movie != null)
                        {
                            movies.Add(movie);
                        }
                    }

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 2000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for advanced search; falling back to DB: {ex.Message}");
            }

            return await AdvancedSearchInDatabaseAsync(query, minRating, releaseYear, page, pageSize);
        }

        // Fetches movies by year with pagination
        public async Task<PaginatedResult<Movie>> GetMoviesByYearAsync(int year, int page = 1, int pageSize = DefaultPageSize)
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

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 1000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for movies by year; falling back to DB: {ex.Message}");
            }

            var query = _context.Movies.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == year);
            return await GetPaginatedResultAsync(query.OrderByDescending(m => m.VoteAverage), page, pageSize);
        }

        // Fetches movies by minimum rating with pagination
        public async Task<PaginatedResult<Movie>> GetMoviesByMinRatingAsync(int minRating, int page = 1, int pageSize = DefaultPageSize)
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

                    int estimatedTotal = Math.Min(page * pageSize + (moviesFromApi.Count == pageSize ? pageSize : 0), 1000);
                    return new PaginatedResult<Movie>(movies, estimatedTotal, page, pageSize);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API unavailable for movies by rating; falling back to DB: {ex.Message}");
            }

            var query = _context.Movies.Where(m => (int)Math.Round(m.VoteAverage) >= minRating);
            return await GetPaginatedResultAsync(query.OrderByDescending(m => m.VoteAverage), page, pageSize);
        }

        // Retrieves popular movies from the database with pagination
        private async Task<PaginatedResult<Movie>> GetPopularMoviesFromDatabaseAsync(int page = 1, int pageSize = DefaultPageSize)
        {
            var query = _context.Movies.OrderByDescending(m => m.VoteAverage).ThenByDescending(m => m.VoteCount);
            return await GetPaginatedResultAsync(query, page, pageSize);
        }

        // Retrieves movies matching the search query from the database with pagination
        private async Task<PaginatedResult<Movie>> SearchMoviesInDatabaseAsync(string query, int page = 1, int pageSize = DefaultPageSize)
        {
            var moviesQuery = _context.Movies
                .Where(m => m.Title.ToLower().Contains(query.ToLower()) ||
                            m.Overview.ToLower().Contains(query.ToLower()))
                .OrderByDescending(m => m.VoteAverage);

            return await GetPaginatedResultAsync(moviesQuery, page, pageSize);
        }

        // Retrieves top-rated movies from the database with pagination
        private async Task<PaginatedResult<Movie>> GetTopRatedMoviesFromDatabaseAsync(int page = 1, int pageSize = DefaultPageSize)
        {
            var query = _context.Movies
                .Where(m => m.VoteAverage >= 8.0)
                .OrderByDescending(m => m.VoteAverage);

            return await GetPaginatedResultAsync(query, page, pageSize);
        }

        // Performs an advanced search on the database with optional filters and pagination
        private async Task<PaginatedResult<Movie>> AdvancedSearchInDatabaseAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1, int pageSize = DefaultPageSize)
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

            return await GetPaginatedResultAsync(moviesQuery.OrderByDescending(m => m.VoteAverage), page, pageSize);
        }

        // Helper method to create a paginated result from a query
        private async Task<PaginatedResult<Movie>> GetPaginatedResultAsync(IQueryable<Movie> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Movie>(items, totalCount, page, pageSize);
        }

        // Retrieves or creates a movie in the database
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