using System.Text.Json;
using System.Text.Json.Serialization;

namespace HasherDataObjects.Models
{
    public class HashableFile : AuditableThing
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Hash { get; set; } = string.Empty;

        [JsonIgnore]
        public NestableFolder? Folder { get; set; }
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public RunResults? LastRun { get; set; } = null;
        public JobInfo? LastJob { get; set; } = null!;

        public string Extension { get; set; } = string.Empty;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
