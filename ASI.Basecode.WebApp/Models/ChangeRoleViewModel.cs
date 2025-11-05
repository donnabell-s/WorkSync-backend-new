using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    public class ChangeRoleViewModel
    {
        [JsonPropertyName("email")]
        [Required]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        [Required]
        public string Role { get; set; }
    }
}