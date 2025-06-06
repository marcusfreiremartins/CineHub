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
    }
}