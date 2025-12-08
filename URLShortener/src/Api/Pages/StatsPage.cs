using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using URLShortener.Application.Interfaces;

namespace URLShortener.Api.Pages
{
    public class StatsPageModel : PageModel
    {
        private readonly IURLShortenerService _service;

        public StatsPageModel(IURLShortenerService service)
        {
            _service = service;
        }

        [BindProperty]
        public string ShortCode { get; set; } = string.Empty;

        public string LongUrl { get; set; } = string.Empty;

        public int? Clicks { get; set; }

        public string? Error { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!TryValidateShortCode(ShortCode, out var error))
            {
                Error = error;
                return Page();
            }

            try
            {
                var count = await _service.GetUrlAccessCountAsync(ShortCode);
                Clicks = count;
                LongUrl = await _service.GetLongUrlAsync(ShortCode);
                return Page();
            }
            catch
            {
                Error = "Short code not found";
                return Page();
            }
        }

        private static bool TryValidateShortCode(string shortCode, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                error = "Enter a short code";
                return false;
            }
            if (shortCode.Length > 10)
            {
                error = "Short code too long";
                return false;
            }
            if (!Regex.IsMatch(shortCode, "^[0-9A-Za-z]{1,10}$"))
            {
                error = "Invalid short code";
                return false;
            }
            return true;
        }
    }
}
