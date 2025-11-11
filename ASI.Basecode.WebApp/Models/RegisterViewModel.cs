using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    public class RegisterViewModel
    {
        [JsonPropertyName("firstName")]
        [Required]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        [Required]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        [Required]
        public string Password { get; set; }
    }
}