using System.Text.Json;
using CineHub.Models.DTOs;
using System.Net;
using CineHub.Services.Ranking;
using CineHub.Models;

namespace CineHub.Services
{
    public class TMDbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IRankingService _rankingService;
        private readonly string _baseUrl = "https://api.themoviedb.org/3";

        public TMDbService(HttpClient httpClient, IConfiguration configuration, IRankingService rankingService)
        {
            _httpClient = httpClient;
            _rankingService = rankingService;
            _apiKey = configuration["TMDb:ApiKey"] ?? "demo_key";

            // Configure request timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        // Fetches a list of popular movies from TMDb API
        public async Task<List<MovieDTO>> GetPopularMoviesAsync(int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/movie/popular?api_key={_apiKey}&page={page}&language=pt-BR"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    return result?.Results ?? new List<MovieDTO>();
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error: {response.StatusCode} - {response.ReasonPhrase}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API request timed out.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error to TMDb API: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching popular movies: {ex.Message}");
                throw;
            }
        }

        // Fetches detailed information of a specific movie by its TMDb ID
        public async Task<MovieDTO?> GetMovieDetailsAsync(int tmdbId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/movie/{tmdbId}?api_key={_apiKey}&language=pt-BR"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MovieDTO>(json);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Movie with ID {tmdbId} not found on TMDb API.");
                    return null;
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error for movie details: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API request for movie details timed out.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (details): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching movie details: {ex.Message}");
                throw;
            }
        }

        // Searches movies by query string using TMDb API
        public async Task<List<MovieDTO>> SearchMoviesAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<MovieDTO>();

            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&page={page}&language=pt-BR"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    var movies = result?.Results ?? new List<MovieDTO>();

                    // Convert DTOs to Movies for ranking, then back to DTOs
                    if (movies.Any())
                    {
                        var movieEntities = ConvertDtosToMovies(movies);
                        var rankedMovies = _rankingService.ApplySearchRelevanceRanking(movieEntities, query);
                        return ConvertMoviesToDtos(rankedMovies);
                    }

                    return movies;
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error on search: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API search request timed out.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (search): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while searching movies: {ex.Message}");
                throw;
            }
        }

        // Fetches top-rated movies with optional pagination
        public async Task<List<MovieDTO>> GetTopRatedMoviesAsync(int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/movie/top_rated?api_key={_apiKey}&page={page}&language=pt-BR"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    return result?.Results?.Where(m => m.VoteAverage >= 8.0)
                                         ?.OrderByDescending(m => m.VoteAverage)
                                         ?.ToList() ?? new List<MovieDTO>();
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error for top rated movies: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API top-rated request timed out.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (top rated): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching top rated movies: {ex.Message}");
                throw;
            }
        }

        // Fetches movies by specific year
        public async Task<List<MovieDTO>> GetMoviesByYearAsync(int year, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/discover/movie?api_key={_apiKey}" +
                    $"&primary_release_year={year}" +
                    $"&page={page}&language=pt-BR" +
                    $"&sort_by=vote_count.desc"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    var movies = result?.Results ?? new List<MovieDTO>();

                    // Apply Wilson Score ranking for year-based searches
                    if (movies.Any())
                    {
                        var movieEntities = ConvertDtosToMovies(movies);
                        var rankedMovies = _rankingService.ApplyWilsonScoreRanking(movieEntities);
                        return ConvertMoviesToDtos(rankedMovies);
                    }

                    return movies;
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error for year search: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API request timed out for year search.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (year search): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while searching movies by year: {ex.Message}");
                throw;
            }
        }

        // Fetches movies with a minimum rating - APPLIES WILSON SCORE RANKING
        public async Task<List<MovieDTO>> GetMoviesByMinRatingAsync(double minRating, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/discover/movie?api_key={_apiKey}" +
                    $"&vote_average.gte={minRating}&page={page}" +
                    $"&language=pt-BR" +
                    $"&sort_by=vote_count.desc"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    var movies = result?.Results ?? new List<MovieDTO>();

                    // Apply Wilson Score ranking for rating-based searches
                    if (movies.Any())
                    {
                        var movieEntities = ConvertDtosToMovies(movies);
                        var rankedMovies = _rankingService.ApplyWilsonScoreRanking(movieEntities);
                        return ConvertMoviesToDtos(rankedMovies);
                    }

                    return movies;
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error for min rating search: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API request timed out for min rating search.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (min rating): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while searching movies by min rating: {ex.Message}");
                throw;
            }
        }

        // Advanced discover search combining multiple filters - APPLIES WILSON SCORE RANKING
        public async Task<List<MovieDTO>> DiscoverMoviesAsync(int? year = null, double? minRating = null, int page = 1)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"api_key={_apiKey}",
                    $"page={page}",
                    $"language=pt-BR",
                    $"sort_by=vote_count.desc"
                };

                if (year.HasValue)
                    queryParams.Add($"primary_release_year={year.Value}");

                if (minRating.HasValue)
                    queryParams.Add($"vote_average.gte={minRating.Value}");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"{_baseUrl}/discover/movie?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    var movies = result?.Results ?? new List<MovieDTO>();

                    // Apply Wilson Score ranking for discovery searches
                    if (movies.Any())
                    {
                        var movieEntities = ConvertDtosToMovies(movies);
                        var rankedMovies = _rankingService.ApplyWilsonScoreRanking(movieEntities);
                        return ConvertMoviesToDtos(rankedMovies);
                    }

                    return movies;
                }
                else
                {
                    Console.WriteLine($"TMDb API returned error on discover search: {response.StatusCode}");
                    throw new HttpRequestException($"API error: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("TMDb API request timed out for discover search.");
                throw new TimeoutException("TMDb API timeout", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection error with TMDb API (discover): {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while discovering movies: {ex.Message}");
                throw;
            }
        }

        // Search movies by query and optional filters - APPLIES SEARCH RELEVANCE OR WILSON SCORE RANKING
        public async Task<List<MovieDTO>> SearchMoviesWithFiltersAsync(string? query = null, int? year = null, double? minRating = null, int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    var searchResults = await SearchMoviesAsync(query, page);

                    var filteredResults = searchResults.AsEnumerable();

                    if (year.HasValue)
                        filteredResults = filteredResults.Where(m => DateTime.TryParse(m.ReleaseDate, out var date) && date.Year == year.Value);

                    if (minRating.HasValue)
                        filteredResults = filteredResults.Where(m => m.VoteAverage >= minRating.Value);

                    var finalResults = filteredResults.ToList();

                    // Apply additional ranking after filtering if we have results
                    if (finalResults.Any())
                    {
                        var movieEntities = ConvertDtosToMovies(finalResults);
                        var rankedMovies = _rankingService.ApplySearchRelevanceRanking(movieEntities, query);
                        return ConvertMoviesToDtos(rankedMovies);
                    }

                    return finalResults;
                }
                catch
                {
                    Console.WriteLine("Search failed with query, attempting discover without query.");
                    throw;
                }
            }

            return await DiscoverMoviesAsync(year, minRating, page);
        }

        // Checks if TMDb API is up and responding
        public async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/configuration?api_key={_apiKey}",
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token
                );

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to convert MovieDTO list to Movie list for ranking
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
                VoteAverage = dto.VoteAverage,
                VoteCount = dto.VoteCount,
                LastUpdated = DateTime.UtcNow
            }).ToList();
        }

        // Helper method to convert Movie list back to MovieDTO list
        private List<MovieDTO> ConvertMoviesToDtos(List<Movie> movies)
        {
            return movies.Select(movie => new MovieDTO
            {
                Id = movie.TMDbId,
                Title = movie.Title ?? string.Empty,
                Overview = movie.Overview ?? string.Empty,
                ReleaseDate = movie.ReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                PosterPath = movie.PosterPath ?? string.Empty,
                VoteAverage = movie.VoteAverage,
                VoteCount = movie.VoteCount
            }).ToList();
        }
    }
}