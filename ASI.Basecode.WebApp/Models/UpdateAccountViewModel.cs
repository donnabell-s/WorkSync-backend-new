using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    public class UpdateAccountViewModel
    {
        [JsonPropertyName("fname")]
        public string Fname { get; set; }

        [JsonPropertyName("lname")]
        public string Lname { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}

