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
            //var moviesFromDb = await _context.Movies
            //.OrderByDescending(m => m.VoteCount)
            //.Take(20)
            // .ToListAsync();

            // if (moviesFromDb.Any())
            // {
            //return moviesFromDb;
            //  }

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
        // First tries to search in local database,
        // if none found, queries TMDb API and saves new movies locally.
        public async Task<List<Movie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Movie>();

            // var moviesFromDb = await _context.Movies
            // .Where(m => m.Title.Contains(query))
            // .Take(20)
            //  .ToListAsync();

            //if (moviesFromDb.Any())
            //  {
            // return moviesFromDb;
            // }

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

        // Adicione estes métodos à sua classe MovieService existente

        // Método para pesquisa avançada com filtros
        public async Task<List<Movie>> AdvancedSearchAsync(string? query = null, int? minRating = null, int? releaseYear = null, int page = 1)
        {
            List<Movie> movies = new List<Movie>();

            // Se há uma query de texto, pesquise primeiro
            if (!string.IsNullOrWhiteSpace(query))
            {
                movies = await SearchMoviesAsync(query, page);
            }
            else
            {
                // Se não há query, pegue filmes populares para filtrar
                movies = await GetPopularMoviesAsync(page);
            }

            // Aplique os filtros
            var filteredMovies = movies.AsQueryable();

            // Filtro por nota mínima
            if (minRating.HasValue)
            {
                filteredMovies = filteredMovies.Where(m => (int)Math.Round(m.VoteAverage) >= minRating.Value);
            }

            // Filtro por ano de lançamento
            if (releaseYear.HasValue)
            {
                filteredMovies = filteredMovies.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);
            }

            return filteredMovies.ToList();
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

            // Se não encontrar localmente, busca filmes populares e filtra
            var popularMovies = await GetPopularMoviesAsync(page);
            return popularMovies.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == year).ToList();
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

            // Se não encontrar localmente, busca filmes populares e filtra
            var popularMovies = await GetPopularMoviesAsync(page);
            return popularMovies.Where(m => (int)Math.Round(m.VoteAverage) >= minRating).ToList();
        }
    }
}