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

        public async Task<List<Movie>> GetPopularMoviesAsync(int page = 1)
        {
            // Primeiro tenta buscar do banco
            //var moviesFromDb = await _context.Movies
                //.OrderByDescending(m => m.VoteCount)
                //.Take(20)
               // .ToListAsync();

           // if (moviesFromDb.Any())
           // {
                //return moviesFromDb;
          //  }

            // Se não tem no banco, busca da API
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

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task<Movie?> GetMovieByTMDbIdAsync(int tmdbId)
        {
            return await _context.Movies.FirstOrDefaultAsync(m => m.TMDbId == tmdbId);
        }

        public async Task<List<Movie>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Movie>();

            // Busca no banco primeiro
           // var moviesFromDb = await _context.Movies
               // .Where(m => m.Title.Contains(query))
               // .Take(20)
              //  .ToListAsync();

            //if (moviesFromDb.Any())
          //  {
               // return moviesFromDb;
           // }

            // Se não encontrou no banco, busca na API
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