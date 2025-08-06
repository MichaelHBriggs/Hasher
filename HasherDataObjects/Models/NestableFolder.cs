using System.Text.Json;

namespace HasherDataObjects.Models
{
    public class NestableFolder : AuditableThing
    {
        public string Name { get; set; } = string.Empty;
        public NestableFolder? Parent { get; set; }
        public List<NestableFolder> Children { get; set; } = [];
        public Drive? Drive { get; set; }

        public List<HashableFile> Files { get; set; } = [];
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
