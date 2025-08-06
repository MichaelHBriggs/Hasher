using System.Text.Json.Serialization;

namespace HasherDataObjects.Configuration
{
    public class Folder
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("RootFolder")]
        public string RootFolder { get; set; } = string.Empty;
        [JsonPropertyName("Extensions")]
        public List<string> Extensions { get; set; } = [];
    }
}
