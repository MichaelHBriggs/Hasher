using System.Text.Json.Serialization;

namespace HasherDataObjects.Configuration
{
    public class Settings
    {
        public List<Folder> Folders { get; set; } = [];
        
        [JsonPropertyName("Email")]
        public EmailSettings Email { get; set; } = new EmailSettings();
        public string HasherDBConnectionString { get; set; } = string.Empty;
        public int TakeSize { get; set; } = -1;
    }
}
