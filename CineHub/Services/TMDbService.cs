using System.Text.Json;
using CineHub.Models.DTOs;

namespace CineHub.Services
{
    public class TMDbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.themoviedb.org/3";

        public TMDbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["TMDb:ApiKey"] ?? "demo_key";
        }

        // Fetches a paginated list of popular movies from TMDb API
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar filmes populares: {ex.Message}");
            }

            return new List<MovieDTO>();
        }

        // Retrieves detailed information for a movie by its TMDb ID
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar detalhes do filme: {ex.Message}");
            }

            return null;
        }

        // Searches for movies by a query string using TMDb API
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
                    return result?.Results ?? new List<MovieDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar filmes: {ex.Message}");
            }

            return new List<MovieDTO>();
        }

        // Top Rated Movies
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching top rated movies: {ex.Message}");
            }

            return new List<MovieDTO>();
        }


        // Busca filmes por ano específico usando discover endpoint
        public async Task<List<MovieDTO>> GetMoviesByYearAsync(int year, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/discover/movie?api_key={_apiKey}&primary_release_year={year}&page={page}&language=pt-BR&sort_by=popularity.desc"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    return result?.Results ?? new List<MovieDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar filmes por ano: {ex.Message}");
            }

            return new List<MovieDTO>();
        }

        // Busca filmes por nota mínima usando discover endpoint
        public async Task<List<MovieDTO>> GetMoviesByMinRatingAsync(double minRating, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/discover/movie?api_key={_apiKey}&vote_average.gte={minRating}&page={page}&language=pt-BR&sort_by=popularity.desc&vote_count.gte=100"
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    return result?.Results ?? new List<MovieDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar filmes por nota: {ex.Message}");
            }

            return new List<MovieDTO>();
        }

        // Busca avançada combinando múltiplos filtros usando discover endpoint
        public async Task<List<MovieDTO>> DiscoverMoviesAsync(int? year = null, double? minRating = null, int page = 1)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"api_key={_apiKey}",
                    $"page={page}",
                    "language=pt-BR",
                    "sort_by=popularity.desc"
                };

                if (year.HasValue)
                {
                    queryParams.Add($"primary_release_year={year.Value}");
                }

                if (minRating.HasValue)
                {
                    queryParams.Add($"vote_average.gte={minRating.Value}");
                    queryParams.Add("vote_count.gte=50");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"{_baseUrl}/discover/movie?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TMDbResponse>(json);
                    return result?.Results ?? new List<MovieDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer discover de filmes: {ex.Message}");
            }

            return new List<MovieDTO>();
        }

        // Busca combinada: query + filtros
        public async Task<List<MovieDTO>> SearchMoviesWithFiltersAsync(string? query = null, int? year = null, double? minRating = null, int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchResults = await SearchMoviesAsync(query, page);

                var filteredResults = searchResults.AsEnumerable();

                if (year.HasValue)
                {
                    filteredResults = filteredResults.Where(m =>
                    {
                        if (DateTime.TryParse(m.ReleaseDate, out var date))
                        {
                            return date.Year == year.Value;
                        }
                        return false;
                    });
                }

                if (minRating.HasValue)
                {
                    filteredResults = filteredResults.Where(m => m.VoteAverage >= minRating.Value);
                }

                return filteredResults.ToList();
            }

            return await DiscoverMoviesAsync(year, minRating, page);
        }
    }
}