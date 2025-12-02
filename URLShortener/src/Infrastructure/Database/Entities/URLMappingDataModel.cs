using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URLShortener.Infrastructure.Database.Entities
{
    // Defines the structure of the UrlMapping table in PostgreSQL
    public class UrlMappingDataModel
    {
        // Primary Key for the table. Using GUID for robust identification.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // The short, unique identifier used in the URL (e.g., "aBc1D").
        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        // The original destination URL.
        [Required]
        [MaxLength(2048)] // Standard maximum length for a URL
        public string LongUrl { get; set; } = string.Empty;

        // Counter for how many times the short code has been clicked.
        public int Clicks { get; set; } = 0;

        // Timestamp for when the short code was created.
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Optional: Timestamp for when the short code should expire.
        public DateTime? ExpirationDate { get; set; }
    }
}