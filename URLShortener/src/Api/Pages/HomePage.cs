using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using URLShortener.Application.Interfaces;

namespace URLShortener.Api.Pages
{
    public class HomePageModel : PageModel
    {
        private readonly IURLShortenerService _service;
        private readonly ILogger<HomePageModel> _logger;
        private const string Green = "\x1B[32m";
        private const string Reset = "\x1B[0m";

        public HomePageModel(IURLShortenerService service, ILogger<HomePageModel> logger)
        {
            _service = service;
            _logger = logger;
        }

        [BindProperty]
        public string OriginalUrl { get; set; } = string.Empty;

        public string? ShortCode { get; set; }

        public string? Error { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!TryValidateLongUrl(OriginalUrl, out var error))
            {
                Error = error;
                return Page();
            }

            try
            {
                var code = await _service.GenerateShortCodeAsync(OriginalUrl);
                ShortCode = code;
                _logger.LogInformation($"{Green}Generated short code {code} for URL {OriginalUrl}{Reset}");
                return Page();
            }
            catch
            {
                Error = "Failed to generate short URL";
                return Page();
            }
        }

        private static bool TryValidateLongUrl(string url, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                error = "Enter a URL";
                return false;
            }
            if (url.Length > 2048)
            {
                error = "URL too long";
                return false;
            }
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                error = "Invalid URL";
                return false;
            }
            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                error = "URL must use http or https";
                return false;
            }
            return true;
        }
    }
}
