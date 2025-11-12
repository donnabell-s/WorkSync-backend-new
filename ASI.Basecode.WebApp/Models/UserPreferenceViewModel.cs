using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// User Preference View Model
    /// </summary>
    public class UserPreferenceViewModel
    {
        [JsonPropertyName("prefId")]
        public int? PrefId { get; set; }

        [JsonPropertyName("userRefId")]
        public int? UserRefId { get; set; }

        [JsonPropertyName("bookingEmailConfirm")]
        public bool? BookingEmailConfirm { get; set; }

        [JsonPropertyName("cancellationNotif")]
        public bool? CancellationNotif { get; set; }

        [JsonPropertyName("bookingReminder")]
        public bool? BookingReminder { get; set; }

        [JsonPropertyName("reminderTimeMinutes")]
        [Range(0, 1440, ErrorMessage = "Reminder time must be between 0 and 1440 minutes.")]
        public int? ReminderTimeMinutes { get; set; }

        [JsonPropertyName("bookingDefaultMinutes")]
        [Range(1, 1440, ErrorMessage = "Default booking duration must be between 1 and 1440 minutes.")]
        public int? BookingDefaultMinutes { get; set; }

        [JsonPropertyName("rawJson")]
        public string RawJson { get; set; }
    }
}

