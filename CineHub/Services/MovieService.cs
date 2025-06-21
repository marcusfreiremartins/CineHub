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

        // Retrieves popular movies, first tries from the local database,
        // if none found, fetches from the external TMDb API and saves them locally.
        public async Task<List<Movie>> GetPopularMoviesAsync(int page = 1)
        {
            var moviesFromApi = await _tmdbService.GetPopularMoviesAsync(page);
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

        // Gets a movie by its local database ID
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        // Gets a movie by its TMDb external ID
        public async Task<Movie?> GetMovieByTMDbIdAsync(int tmdbId)
        {
            return await _context.Movies.FirstOrDefaultAsync(m => m.TMDbId == tmdbId);
        }

        // Searches for movies by title query
        public async Task<List<Movie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Movie>();

            var moviesFromApi = await _tmdbService.SearchMoviesAsync(query, page);
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

        // Gets top-rated movies from the API and ensures they're saved in the local database.
        public async Task<List<Movie>> GetTopRatedMoviesAsync()
        {
            var moviesFromApi = await _tmdbService.GetTopRatedMoviesAsync();
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

        public async Task<List<Movie>> AdvancedSearchAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1)
        {
            List<MovieDTO> moviesFromApi;

            // Estratégia de busca inteligente
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Se há query de texto, usa busca com filtros combinados
                moviesFromApi = await _tmdbService.SearchMoviesWithFiltersAsync(
                    query, releaseYear, minRating.HasValue ? (double)minRating.Value : null, page);
            }
            else if (releaseYear.HasValue && minRating.HasValue)
            {
                // Busca por ano E nota mínima
                moviesFromApi = await _tmdbService.DiscoverMoviesAsync(
                    releaseYear.Value, (double)minRating.Value, page);
            }
            else if (releaseYear.HasValue)
            {
                // Busca apenas por ano
                moviesFromApi = await _tmdbService.GetMoviesByYearAsync(releaseYear.Value, page);
            }
            else if (minRating.HasValue)
            {
                // Busca apenas por nota mínima
                moviesFromApi = await _tmdbService.GetMoviesByMinRatingAsync((double)minRating.Value, page);
            }
            else
            {
                // Sem filtros, retorna populares
                moviesFromApi = await _tmdbService.GetPopularMoviesAsync(page);
            }

            // Converte DTOs para entidades Movie
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

        // Método para obter filmes por ano específico
        public async Task<List<Movie>> GetMoviesByYearAsync(int year, int page = 1)
        {
            // Primeiro tenta buscar no banco local
            var moviesFromDb = await _context.Movies
                .Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == year)
                .OrderByDescending(m => m.VoteAverage)
                .Take(20)
                .ToListAsync();

            if (moviesFromDb.Any())
            {
                return moviesFromDb;
            }

            // Busca na API usando o novo método específico
            var moviesFromApi = await _tmdbService.GetMoviesByYearAsync(year, page);
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

        // Método para obter filmes por nota mínima
        public async Task<List<Movie>> GetMoviesByMinRatingAsync(int minRating, int page = 1)
        {
            // Primeiro tenta buscar no banco local
            var moviesFromDb = await _context.Movies
                .Where(m => (int)Math.Round(m.VoteAverage) >= minRating)
                .OrderByDescending(m => m.VoteAverage)
                .Take(20)
                .ToListAsync();

            if (moviesFromDb.Any())
            {
                return moviesFromDb;
            }

            // Busca na API usando o novo método específico
            var moviesFromApi = await _tmdbService.GetMoviesByMinRatingAsync((double)minRating, page);
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

        // Checks if movie exists in DB by TMDbId, otherwise creates and saves it
        private async Task<Movie?> GetOrCreateMovieAsync(MovieDTO movieDto)
        {
            var existingMovie = await _context.Movies
                .FirstOrDefaultAsync(m => m.TMDbId == movieDto.Id);

            if (existingMovie != null)
            {
                return existingMovie;
            }

            var movie = new Movie
            {
                TMDbId = movieDto.Id,
                Title = movieDto.Title,
                Overview = movieDto.Overview,
                ReleaseDate = DateTime.TryParse(movieDto.ReleaseDate, out var date)
                    ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                    : null,
                PosterPath = movieDto.PosterPath,
                VoteAverage = movieDto.VoteAverage,
                VoteCount = movieDto.VoteCount,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return movie;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar filme: {ex.Message}");
                return null;
            }
        }
    }
}