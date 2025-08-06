using System.Text.Json.Serialization;

namespace HasherDataObjects.Configuration
{
    public class EmailSettings
    {
        [JsonPropertyName("SMTPServerName")]
        public string SMTPServerName { get; set; } = string.Empty;
        [JsonPropertyName("SMTPPort")]
        public int SmtpPort { get; set; } = 587;
        [JsonPropertyName("Username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("AppPassword")]
        public string AppPassword { get; set; } = string.Empty;
        [JsonPropertyName("EmailFrom")]
        public string EmailFrom { get; set; } = string.Empty;
        [JsonPropertyName("EmailTo")]
        public string EmailTo { get; set; } = string.Empty;
    }
}
