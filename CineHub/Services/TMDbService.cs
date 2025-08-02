using System.Net;
using System.Text.Json;
using CineHub.Models;
using CineHub.Models.DTOs;
using CineHub.Services.Ranking;

namespace CineHub.Services
{
    public class TMDbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IRankingService _rankingService;
        private readonly ILogger<TMDbService> _logger;
        private readonly string _baseUrl;

        public TMDbService(
            HttpClient httpClient,
            IConfiguration configuration,
            IRankingService rankingService,
            ILogger<TMDbService> logger)
        {
            _httpClient = httpClient;
            _rankingService = rankingService;
            _logger = logger;
            _apiKey = configuration["TMDb:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "TMDb: ApiKey not found.");
            _baseUrl = configuration["TMDb:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "TMDb: BaseUrl not found.");

            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        // Gets a list of popular movies from TMDb
        public async Task<List<MovieDTO>> GetPopularMoviesAsync(int page = 1)
        {
            var queryParams = new Dictionary<string, string> { { "page", page.ToString() } };
            var response = await SendRequestAsync<TMDbResponse>("/movie/popular", queryParams);
            return response?.Results ?? new List<MovieDTO>();
        }

        // Gets detailed information of a specific movie by its TMDb ID
        public async Task<MovieDTO?> GetMovieDetailsAsync(int tmdbId)
        {
            return await SendRequestAsync<MovieDTO>($"/movie/{tmdbId}");
        }

        // Searches movies by name and applies relevance ranking
        public async Task<List<MovieDTO>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<MovieDTO>();

            var queryParams = new Dictionary<string, string>
            {
                { "query", query },
                { "page", page.ToString() }
            };

            var response = await SendRequestAsync<TMDbResponse>("/search/movie", queryParams);
            var movies = response?.Results ?? new List<MovieDTO>();

            if (movies.Any())
            {
                var movieEntities = ConvertDtosToMovies(movies);
                var rankedMovies = _rankingService.ApplySearchRelevanceRanking(movieEntities, query);
                return ConvertMoviesToDtos(rankedMovies);
            }

            return movies;
        }

        // Gets top-rated movies (with rating >= 8), ordered by rating
        public async Task<List<MovieDTO>> GetTopRatedMoviesAsync(int page = 1)
        {
            var queryParams = new Dictionary<string, string> { { "page", page.ToString() } };
            var response = await SendRequestAsync<TMDbResponse>("/movie/top_rated", queryParams);

            return response?.Results?
                .Where(m => m.VoteAverage >= 8.0)
                .OrderByDescending(m => m.VoteAverage)
                .ToList() ?? new List<MovieDTO>();
        }

        // Discovers movies with optional filters for year and minimum rating, applying Wilson score ranking
        public async Task<List<MovieDTO>> DiscoverMoviesAsync(int? year = null, double? minRating = null, int page = 1)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "sort_by", "vote_count.desc" }
            };

            if (year.HasValue)
                queryParams.Add("primary_release_year", year.Value.ToString());

            if (minRating.HasValue)
                queryParams.Add("vote_average.gte", minRating.Value.ToString("F1"));

            var response = await SendRequestAsync<TMDbResponse>("/discover/movie", queryParams);
            var movies = response?.Results ?? new List<MovieDTO>();

            if (movies.Any())
            {
                var movieEntities = ConvertDtosToMovies(movies);
                var rankedMovies = _rankingService.ApplyWilsonScoreRanking(movieEntities);
                return ConvertMoviesToDtos(rankedMovies);
            }

            return movies;
        }

        // Searches movies with filters for query, year, and minimum rating
        public async Task<List<MovieDTO>> SearchMoviesWithFiltersAsync(string? query = null, int? year = null, double? minRating = null, int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchResults = await SearchMoviesAsync(query, page);

                var filteredResults = searchResults.AsEnumerable();
                if (year.HasValue)
                    filteredResults = filteredResults.Where(m => DateTime.TryParse(m.ReleaseDate, out var date) && date.Year == year.Value);
                if (minRating.HasValue)
                    filteredResults = filteredResults.Where(m => m.VoteAverage >= minRating.Value);

                return filteredResults.ToList();
            }

            return await DiscoverMoviesAsync(year, minRating, page);
        }

        public async Task<(List<PersonDTO> Cast, List<PersonDTO> Crew)> GetPersonMovieCreditsAsync(int personId)
        {
            var response = await SendRequestAsync<MovieCreditsResponse>($"/person/{personId}/movie_credits");
            return (response?.Cast ?? new List<PersonDTO>(), response?.Crew ?? new List<PersonDTO>());
        }

        // Gets details of a person (actor/director) by TMDb person ID
        public async Task<PersonDTO?> GetPersonDetailsAsync(int personId)
        {
            return await SendRequestAsync<PersonDTO>($"/person/{personId}");
        }

        // Gets movie credits (cast and crew) for a given movie ID
        public async Task<(List<PersonDTO> Cast, List<PersonDTO> Crew)> GetMovieCreditsAsync(int movieId)
        {
            var response = await SendRequestAsync<MovieCreditsResponse>($"/movie/{movieId}/credits");
            return (response?.Cast ?? new List<PersonDTO>(), response?.Crew ?? new List<PersonDTO>());
        }

        // Checks if the TMDb API is available without throwing exceptions
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await SendRequestAsync<object>("/configuration", useGlobalErrorHandling: false);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        // Sends a request to the TMDb API and handles errors gracefully
        private async Task<T?> SendRequestAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null, bool useGlobalErrorHandling = true) where T : class
        {
            queryParams ??= new Dictionary<string, string>();
            queryParams["api_key"] = _apiKey;
            queryParams["language"] = "pt-BR";

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var fullUrl = $"{_baseUrl}{endpoint}?{queryString}";

            try
            {
                var response = await _httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Resource not found (404) at URL: {ApiUrl}", fullUrl);
                    return null;
                }

                _logger.LogError("API returned an error. Status: {StatusCode}, URL: {ApiUrl}", response.StatusCode, fullUrl);

                if (useGlobalErrorHandling)
                    response.EnsureSuccessStatusCode();

                return null;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "API request timed out: {ApiUrl}", fullUrl);
                if (useGlobalErrorHandling) throw new TimeoutException("API request timed out", ex);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Connection error while accessing the API: {ApiUrl}", fullUrl);
                if (useGlobalErrorHandling) throw;
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON from the API: {ApiUrl}", fullUrl);
                if (useGlobalErrorHandling) throw;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while calling the API: {ApiUrl}", fullUrl);
                if (useGlobalErrorHandling) throw;
                return null;
            }
        }

        // Converts a list of movie DTOs to Movie entities
        private List<Movie> ConvertDtosToMovies(List<MovieDTO> dtos)
        {
            return dtos.Select(dto => new Movie
            {
                TMDbId = dto.Id,
                Title = dto.Title ?? string.Empty,
                Overview = dto.Overview ?? string.Empty,
                ReleaseDate = !string.IsNullOrEmpty(dto.ReleaseDate) && DateTime.TryParse(dto.ReleaseDate, out var date)
                    ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                    : null,
                PosterPath = dto.PosterPath ?? string.Empty,
                BackdropPath = dto.BackdropPath ?? string.Empty,
                VoteAverage = dto.VoteAverage,
                VoteCount = dto.VoteCount,
                LastUpdated = DateTime.UtcNow
            }).ToList();
        }

        // Converts a list of Movie entities to movie DTOs
        private List<MovieDTO> ConvertMoviesToDtos(List<Movie> movies)
        {
            return movies.Select(movie => new MovieDTO
            {
                Id = movie.TMDbId,
                Title = movie.Title ?? string.Empty,
                Overview = movie.Overview ?? string.Empty,
                ReleaseDate = movie.ReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                PosterPath = movie.PosterPath ?? string.Empty,
                BackdropPath = movie.BackdropPath ?? string.Empty,
                VoteAverage = movie.VoteAverage,
                VoteCount = movie.VoteCount
            }).ToList();
        }
    }
}