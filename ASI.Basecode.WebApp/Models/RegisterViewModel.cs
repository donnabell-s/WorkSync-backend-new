using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    public class RegisterViewModel
    {
        [JsonPropertyName("fullName")]
        [Required]
        public string FullName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        [Required]
        public string Password { get; set; }
    }
}