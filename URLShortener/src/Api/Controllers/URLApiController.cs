using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using URLShortener.Application.Exceptions;
using URLShortener.Application.Interfaces;

namespace URLShortener.Api.Controllers
{
    [ApiController]
    [Route("api/urls")]
    public class URLApiController : ControllerBase
    {
        private readonly IURLShortenerService _urlShortenerService;
        private readonly ILogger<URLApiController> _logger;
        private const string Green = "\x1B[32m";
        private const string Reset = "\x1B[0m";

        public URLApiController(IURLShortenerService urlShortenerService, ILogger<URLApiController> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CreateShortResponse>> CreateShortCode([FromBody] CreateShortRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.OriginalUrl))
            {
                return BadRequest("originalUrl is required");
            }

            if (!TryValidateLongUrl(request.OriginalUrl, out var uriValidationError))
            {
                return BadRequest(uriValidationError);
            }

            try
            {
                _logger.LogInformation($"{Green}Create short code requested for URL {request.OriginalUrl}{Reset}");
                var shortCode = await _urlShortenerService.GenerateShortCodeAsync(request.OriginalUrl);
                _logger.LogInformation($"{Green}Generated short code {shortCode} for URL {request.OriginalUrl}{Reset}");
                if (string.IsNullOrEmpty(shortCode))
                {
                    return StatusCode(500, "Failed to generate short code");
                }

                var response = new CreateShortResponse { ShortCode = shortCode!, OriginalUrl = request.OriginalUrl };
                return Created($"/api/urls/{shortCode}", response);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error while generating short code");
            }
        }


        [HttpGet("{shortCode}/stats")]
        public async Task<ActionResult<UrlStatsResponse>> GetUrlAccessCountAsync(string shortCode)
        {
            if (!TryValidateShortCode(shortCode, out var shortCodeError))
            {
                return BadRequest(shortCodeError);
            }

            try
            {
                var count = await _urlShortenerService.GetUrlAccessCountAsync(shortCode);
                return Ok(new UrlStatsResponse { ShortCode = shortCode, Clicks = count });
            }
            catch (ShortCodeNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error while fetching stats");
            }
        }

        [HttpGet("/r/{shortCode}")]
        public async Task<IActionResult> ResolveAndRedirect(string shortCode)
        {
            _logger.LogInformation($"{Green}Redirect requested for short code {shortCode}{Reset}");
            if (!TryValidateShortCode(shortCode, out var shortCodeError))
            {
                _logger.LogWarning("Invalid short code {ShortCode}: {Error}", shortCode, shortCodeError);
                return BadRequest(shortCodeError);
            }

            try
            {
                var longUrl = await _urlShortenerService.GetLongUrlAsync(shortCode);
                if (string.IsNullOrEmpty(longUrl))
                {
                    _logger.LogWarning("Short code {ShortCode} not found", shortCode);
                    return NotFound();
                }

                if (!TryValidateLongUrl(longUrl!, out var urlError))
                {
                    _logger.LogWarning("Long URL validation failed for short code {ShortCode}: {Error}", shortCode, urlError);
                    return BadRequest(urlError);
                }

                await _urlShortenerService.IncrementClickCountAsync(shortCode);
                _logger.LogInformation($"{Green} Redirecting short code {shortCode} to {longUrl}{Reset}");
                return Redirect(longUrl!);
            }
            catch (ShortCodeNotFoundException)
            {
                _logger.LogWarning("Short code {ShortCode} not found during redirect", shortCode);
                return NotFound();
            }
            catch (Exception)
            {
                _logger.LogError("Unexpected error while redirecting short code {ShortCode}", shortCode);
                return StatusCode(500, "Unexpected error while redirecting");
            }
        }

        private static bool TryValidateShortCode(string shortCode, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                error = "shortCode is required";
                return false;
            }

            if (shortCode.Length > 11)
            {
                error = "shortCode must be at most 11 characters";
                return false;
            }

            if (!Regex.IsMatch(shortCode, "^[0-9A-Za-z]{1,11}$"))
            {
                error = "shortCode must be base62 alphanumeric";
                return false;
            }

            return true;
        }

        private static bool TryValidateLongUrl(string url, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                error = "originalUrl is required";
                return false;
            }

            if (url.Length > 2048)
            {
                error = "originalUrl length exceeds 2048 characters";
                return false;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                error = "originalUrl must be a valid absolute URL";
                return false;
            }

            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                error = "originalUrl scheme must be http or https";
                return false;
            }

            return true;
        }

        public sealed class CreateShortRequest
        {
            public string OriginalUrl { get; set; } = string.Empty;
        }

        public sealed class CreateShortResponse
        {
            public string ShortCode { get; set; } = string.Empty;
            public string OriginalUrl { get; set; } = string.Empty;
        }

        public sealed class GetUrlResponse
        {
            public string ShortCode { get; set; } = string.Empty;
            public string OriginalUrl { get; set; } = string.Empty;
        }

        public sealed class UrlStatsResponse
        {
            public string ShortCode { get; set; } = string.Empty;
            public int Clicks { get; set; }
        }
    }
}
