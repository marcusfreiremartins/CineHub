using Microsoft.AspNetCore.Mvc;

namespace CineHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(IHttpClientFactory httpClientFactory, ILogger<ProxyController> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetImage([FromQuery] string? url)
        {
            if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                _logger.LogWarning("Invalid image URL provided: {Url}", url ?? "null");
                return BadRequest("Invalid image URL provided.");
            }

            // Security check to allow only the TMDB domain
            var uri = new Uri(url);
            if (uri.Host != "image.tmdb.org")
            {
                _logger.LogWarning("Unauthorized domain access attempt: {Domain}", uri.Host);
                return Forbid("Access denied. Only image.tmdb.org domain is allowed.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content == null)
                    {
                        _logger.LogError("Empty response content from TMDB for URL: {Url}", url);
                        return StatusCode(500, "Empty response content.");
                    }

                    var contentType = response.Content.Headers?.ContentType?.ToString();
                    if (!IsValidImageContentType(contentType))
                    {
                        _logger.LogWarning("Invalid content type received: {ContentType} for URL: {Url}", contentType ?? "unknown", url);
                        return BadRequest("Invalid content type received.");
                    }

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(imageBytes, contentType ?? "image/jpeg");
                }

                // Returns the same error status that TMDB returned
                _logger.LogWarning("TMDB returned error status {StatusCode} for URL: {Url}", response.StatusCode, url);
                return StatusCode((int)response.StatusCode, "Image not found.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception when fetching image from URL: {Url}", url);
                return StatusCode(502, $"Bad Gateway. Unable to connect to image service: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout occurred when fetching image from URL: {Url}", url);
                return StatusCode(408, "Request timeout.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request cancelled when fetching image from URL: {Url}", url);
                return StatusCode(499, "Request cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred when fetching image from URL: {Url}", url);
                return StatusCode(500, "Internal server error occurred.");
            }
        }

        private static bool IsValidImageContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/");
        }
    }
}